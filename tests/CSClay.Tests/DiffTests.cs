using CSClay;
using CSClay.Fluent;
using static CSClay.Fluent.Clay;
using Xunit;
using Xunit.Abstractions;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CSClay.Tests;

public class DiffTests
{
    private readonly ITestOutputHelper _output;
    private static readonly string RepoRoot = GetRepoRoot();
    private static readonly string CRepoPath = Path.Combine(RepoRoot, "repro_c");
    private static readonly string ReferencePath = Path.Combine(RepoRoot, "reference");

    private static string GetRepoRoot()
    {
        var current = AppDomain.CurrentDomain.BaseDirectory;
        while (current != null && !File.Exists(Path.Combine(current, "CSClay.slnx")))
        {
            current = Path.GetDirectoryName(current);
        }
        return current ?? throw new DirectoryNotFoundException("Could not find repository root (CSClay.slnx)");
    }

    public DiffTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private struct ExpectedCommand
    {
        public int Type;
        public float X, Y, W, H;
        public int Z;
    }

    private List<ExpectedCommand> RunCImplementation(string layoutCode, int width, int height)
    {
        string cSource = $@"
#define CLAY_IMPLEMENTATION
#include ""{ReferencePath}/clay.h""
#include <stdio.h>
#include <stdlib.h>

Clay_Dimensions MockTextMeasure(Clay_StringSlice text, Clay_TextElementConfig *config, void *userData) {{
    return (Clay_Dimensions) {{ .width = text.length * 10.0f, .height = 20.0f }};
}}

int main() {{
    uint64_t arenaSize = Clay_MinMemorySize();
    Clay_Arena arena = Clay_CreateArenaWithCapacityAndMemory(arenaSize, malloc(arenaSize));
    Clay_Initialize(arena, (Clay_Dimensions) {{ {width}, {height} }}, (Clay_ErrorHandler) {{ 0 }});
    Clay_SetMeasureTextFunction(MockTextMeasure, NULL);

    Clay_BeginLayout();
    {layoutCode}
    Clay_RenderCommandArray commands = Clay_EndLayout();

    for (int i = 0; i < commands.length; i++) {{
        Clay_RenderCommand *cmd = &commands.internalArray[i];
        printf(""%d,%.1f,%.1f,%.1f,%.1f,%d\n"", 
            cmd->commandType, cmd->boundingBox.x, cmd->boundingBox.y, cmd->boundingBox.width, cmd->boundingBox.height, cmd->zIndex);
    }}
    return 0;
}}";

        string sourcePath = Path.Combine(CRepoPath, "temp_test.c");
        string exePath = Path.Combine(CRepoPath, "temp_test");
        File.WriteAllText(sourcePath, cSource);

        var compileProcess = Process.Start(new ProcessStartInfo
        {
            FileName = "gcc",
            Arguments = $"{sourcePath} -o {exePath} -lm",
            RedirectStandardError = true,
            UseShellExecute = false
        });
        compileProcess!.WaitForExit();
        if (compileProcess.ExitCode != 0)
        {
            string error = compileProcess.StandardError.ReadToEnd();
            throw new Exception($"C Compilation failed: {error}");
        }

        var runProcess = Process.Start(new ProcessStartInfo
        {
            FileName = exePath,
            RedirectStandardOutput = true,
            UseShellExecute = false
        });
        
        var results = new List<ExpectedCommand>();
        while (!runProcess!.StandardOutput.EndOfStream)
        {
            string? line = runProcess.StandardOutput.ReadLine();
            if (string.IsNullOrEmpty(line)) continue;
            var parts = line.Split(',');
            results.Add(new ExpectedCommand
            {
                Type = int.Parse(parts[0]),
                X = float.Parse(parts[1]),
                Y = float.Parse(parts[2]),
                W = float.Parse(parts[3]),
                H = float.Parse(parts[4]),
                Z = int.Parse(parts[5])
            });
        }
        runProcess.WaitForExit();

        return results;
    }

    private void AssertParity(string cLayout, Action csLayout, int width, int height)
    {
        var expected = RunCImplementation(cLayout, width, height);

        var arena = new ClayArena(1024 * 1024 * 10);
        var context = new ClayContext(arena);
        UI.SetCurrentContext(context);
        context.TextMeasure = (text, config) => 
        {
            float w = text.Length * 10.0f;
            return new Dimensions(w, 20.0f);
        };

        UI.Begin(arena, new Dimensions(width, height));
        csLayout();
        var actual = UI.End();

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"--- Parity Report ({width}x{height}) ---");
        sb.AppendLine($"Expected Count: {expected.Count}");
        sb.AppendLine($"Actual Count:   {actual.Length}");

        int max = Math.Max(expected.Count, actual.Length);
        for (int i = 0; i < max; i++)
        {
            sb.Append($"Cmd {i}: ");
            if (i < expected.Count)
            {
                var e = expected[i];
                sb.Append($"EXPECTED: Type {e.Type}, BB {{{e.X:F1}, {e.Y:F1}, {e.W:F1}, {e.H:F1}}}, Z {e.Z} | ");
            }
            else
            {
                sb.Append("EXPECTED: (none) | ");
            }

            if (i < actual.Length)
            {
                var a = actual[i];
                string textInfo = "";
                if (a.CommandType == RenderCommandType.Text)
                {
                    var textData = a.RenderData.Text;
                    textInfo = $" | TEXT: \"{context.GetString(textData.TextIndex).Substring(textData.LineStart, textData.LineLength)}\"";
                }
                sb.Append($"ACTUAL: Type {(int)a.CommandType}, BB {{{a.BoundingBox.X:F1}, {a.BoundingBox.Y:F1}, {a.BoundingBox.Width:F1}, {a.BoundingBox.Height:F1}}}, Z {a.ZIndex}{textInfo}");
            }
            else
            {
                sb.Append("ACTUAL: (none)");
            }
            sb.AppendLine();
        }

        _output.WriteLine(sb.ToString());

        Assert.Equal(expected.Count, actual.Length);
        for (int i = 0; i < expected.Count; i++)
        {
            var e = expected[i];
            var a = actual[i];
            Assert.Equal(e.Type, (int)a.CommandType);
            Assert.Equal(e.X, a.BoundingBox.X, 1);
            Assert.Equal(e.Y, a.BoundingBox.Y, 1);
            Assert.Equal(e.W, a.BoundingBox.Width, 1);
            Assert.Equal(e.H, a.BoundingBox.Height, 1);
            Assert.Equal(e.Z, a.ZIndex);
        }
    }

    [Fact]
    public void TestSimpleGrow()
    {
        AssertParity(@"
            CLAY(CLAY_ID(""root""), { .layout = { .sizing = { CLAY_SIZING_FIXED(800), CLAY_SIZING_FIXED(600) } } }) {
                CLAY(CLAY_ID(""child""), { .layout = { .sizing = { CLAY_SIZING_GROW(0, 0), CLAY_SIZING_FIXED(100) } } }) {}
            }",
            () => {
                UI.Container("root", new LayoutConfig { Sizing = new Sizing { Width = SizingAxis.Fixed(800), Height = SizingAxis.Fixed(600) } }, () => {
                    UI.Container("child", new LayoutConfig { Sizing = new Sizing { Width = SizingAxis.Grow(), Height = SizingAxis.Fixed(100) } });
                });
            }, 800, 600);
    }

    [Fact]
    public void TestNestedPadding()
    {
        AssertParity(@"
            CLAY(CLAY_ID(""root""), { 
                .layout = { .padding = { 20, 20, 20, 20 }, .sizing = { CLAY_SIZING_FIXED(400), CLAY_SIZING_FIXED(400) } } 
            }) {
                CLAY(CLAY_ID(""child""), { .layout = { .sizing = { CLAY_SIZING_GROW(0, 0), CLAY_SIZING_GROW(0, 0) } } }) {}
            }",
            () => {
                UI.Container("root", new LayoutConfig { 
                    Padding = new Padding(20, 20, 20, 20),
                    Sizing = new Sizing { Width = SizingAxis.Fixed(400), Height = SizingAxis.Fixed(400) } 
                }, () => {
                    UI.Container("child", new LayoutConfig { Sizing = new Sizing { Width = SizingAxis.Grow(), Height = SizingAxis.Grow() } });
                });
            }, 400, 400);
    }

    [Fact]
    public void TestAlignmentAndColor()
    {
        AssertParity(@"
            CLAY(CLAY_ID(""root""), { 
                .layout = { 
                    .padding = { 10, 10, 10, 10 }, 
                    .childAlignment = { .x = CLAY_ALIGN_X_CENTER, .y = CLAY_ALIGN_Y_CENTER },
                    .sizing = { CLAY_SIZING_FIXED(500), CLAY_SIZING_FIXED(500) } 
                },
                .backgroundColor = { 40, 44, 52, 255 }
            }) {
                CLAY(CLAY_ID(""child""), { 
                    .layout = { .sizing = { CLAY_SIZING_FIXED(100), CLAY_SIZING_FIXED(100) } },
                    .backgroundColor = { 255, 0, 0, 255 }
                }) {}
            }",
            () => {
                UI.Container("root", new LayoutConfig { 
                    Padding = new Padding(10, 10, 10, 10),
                    ChildAlignment = new ChildAlignment { X = LayoutAlignmentX.Center, Y = LayoutAlignmentY.Center },
                    Sizing = new Sizing { Width = SizingAxis.Fixed(500), Height = SizingAxis.Fixed(500) } 
                }, new Color(40, 44, 52), () => {
                    UI.Container("child", new LayoutConfig { 
                        Sizing = new Sizing { Width = SizingAxis.Fixed(100), Height = SizingAxis.Fixed(100) } 
                    }, new Color(255, 0, 0));
                });
            }, 500, 500);
    }

    [Fact]
    public void TestComplexLayout()
    {
        AssertParity(@"
            CLAY(CLAY_ID(""root""), { 
                .layout = { .sizing = { CLAY_SIZING_FIXED(1000), CLAY_SIZING_FIXED(800) }, .layoutDirection = CLAY_TOP_TO_BOTTOM },
                .backgroundColor = { 10, 10, 10, 255 }
            }) {
                CLAY(CLAY_ID(""header""), { 
                    .layout = { .sizing = { CLAY_SIZING_GROW(0, 0), CLAY_SIZING_FIXED(80) }, .padding = { 20, 20, 0, 0 } },
                    .backgroundColor = { 20, 20, 20, 255 }
                }) {
                    CLAY(CLAY_ID(""logo""), { .layout = { .sizing = { CLAY_SIZING_FIXED(100), CLAY_SIZING_GROW(0, 0) } }, .backgroundColor = { 255, 0, 0, 255 } }) {}
                }
                CLAY(CLAY_ID(""body""), { 
                    .layout = { .sizing = { CLAY_SIZING_GROW(0, 0), CLAY_SIZING_GROW(0, 0) }, .layoutDirection = CLAY_LEFT_TO_RIGHT, .childGap = 20, .padding = { 20, 20, 20, 20 } },
                    .backgroundColor = { 30, 30, 30, 255 }
                }) {
                    CLAY(CLAY_ID(""sidebar""), { 
                        .layout = { .sizing = { CLAY_SIZING_PERCENT(0.25f), CLAY_SIZING_GROW(0, 0) } },
                        .backgroundColor = { 40, 40, 40, 255 }
                    }) {}
                    CLAY(CLAY_ID(""content""), { 
                        .layout = { .sizing = { CLAY_SIZING_GROW(0, 0), CLAY_SIZING_GROW(0, 0) }, .layoutDirection = CLAY_TOP_TO_BOTTOM, .childGap = 10 },
                        .backgroundColor = { 50, 50, 50, 255 }
                    }) {
                        CLAY(CLAY_ID(""item1""), { .layout = { .sizing = { CLAY_SIZING_GROW(0, 0), CLAY_SIZING_FIXED(200) } }, .backgroundColor = { 60, 60, 60, 255 } }) {}
                        CLAY(CLAY_ID(""item2""), { .layout = { .sizing = { CLAY_SIZING_GROW(0, 0), CLAY_SIZING_FIXED(200) } }, .backgroundColor = { 80, 80, 80, 255 } }) {}
                    }
                }
            }",
            () => {
                UI.Container("root", new LayoutConfig { 
                    Sizing = new Sizing { Width = SizingAxis.Fixed(1000), Height = SizingAxis.Fixed(800) },
                    LayoutDirection = LayoutDirection.TopToBottom
                }, new Color(10, 10, 10), () => {
                    UI.Container("header", new LayoutConfig { 
                        Sizing = new Sizing { Width = SizingAxis.Grow(), Height = SizingAxis.Fixed(80) },
                        Padding = new Padding(20, 20, 0, 0)
                    }, new Color(20, 20, 20), () => {
                        UI.Container("logo", new LayoutConfig { Sizing = new Sizing { Width = SizingAxis.Fixed(100), Height = SizingAxis.Grow() } }, new Color(255, 0, 0));
                    });
                    UI.Container("body", new LayoutConfig { 
                        Sizing = new Sizing { Width = SizingAxis.Grow(), Height = SizingAxis.Grow() },
                        LayoutDirection = LayoutDirection.LeftToRight,
                        ChildGap = 20,
                        Padding = new Padding(20, 20, 20, 20)
                    }, new Color(30, 30, 30), () => {
                        UI.Container("sidebar", new LayoutConfig { 
                            Sizing = new Sizing { Width = SizingAxis.Percent(0.25f), Height = SizingAxis.Grow() }
                        }, new Color(40, 40, 40));
                        UI.Container("content", new LayoutConfig { 
                            Sizing = new Sizing { Width = SizingAxis.Grow(), Height = SizingAxis.Grow() },
                            LayoutDirection = LayoutDirection.TopToBottom,
                            ChildGap = 10
                        }, new Color(50, 50, 50), () => {
                            UI.Container("item1", new LayoutConfig { Sizing = new Sizing { Width = SizingAxis.Grow(), Height = SizingAxis.Fixed(200) } }, new Color(60, 60, 60));
                            UI.Container("item2", new LayoutConfig { Sizing = new Sizing { Width = SizingAxis.Grow(), Height = SizingAxis.Fixed(200) } }, new Color(80, 80, 80));
                        });
                    });
                });
            }, 1000, 800);
    }

    [Fact]
    public void TestText()
    {
        AssertParity(@"
            CLAY(CLAY_ID(""root""), { .layout = { .sizing = { CLAY_SIZING_FIXED(800), CLAY_SIZING_FIXED(600) } } }) {
                CLAY_TEXT(CLAY_STRING(""Hello, world!""), CLAY_TEXT_CONFIG({ .fontSize = 20 }));
            }",
            () => {
                UI.Container("root", new LayoutConfig { Sizing = new Sizing { Width = SizingAxis.Fixed(800), Height = SizingAxis.Fixed(600) } }, () => {
                    UI.Text("Hello, world!", new TextConfig { FontSize = 20 });
                });
            }, 800, 600);
    }

    [Fact]
    public void TestFloatingAndZIndex()
    {
        AssertParity(@"
            CLAY(CLAY_ID(""root""), { 
                .layout = { .sizing = { CLAY_SIZING_FIXED(800), CLAY_SIZING_FIXED(600) } },
                .backgroundColor = { 40, 44, 52, 255 }
            }) {
                CLAY(CLAY_ID(""parent""), { 
                    .layout = { .sizing = { CLAY_SIZING_FIXED(200), CLAY_SIZING_FIXED(200) } },
                    .backgroundColor = { 100, 100, 100, 255 }
                }) {
                    CLAY(CLAY_ID(""floating""), { 
                        .floating = { 
                            .attachPoints = { .element = CLAY_ATTACH_POINT_LEFT_TOP, .parent = CLAY_ATTACH_POINT_RIGHT_BOTTOM },
                            .zIndex = 100,
                            .attachTo = CLAY_ATTACH_TO_PARENT
                        },
                        .layout = { .sizing = { CLAY_SIZING_FIXED(100), CLAY_SIZING_FIXED(100) } },
                        .backgroundColor = { 255, 255, 0, 255 }
                    }) {}
                }
                CLAY(CLAY_ID(""sibling""), { 
                    .layout = { .sizing = { CLAY_SIZING_FIXED(200), CLAY_SIZING_FIXED(200) } },
                    .backgroundColor = { 200, 200, 200, 255 }
                }) {}
            }",
            () => {
                UI.Container("root", new LayoutConfig { 
                    Sizing = new Sizing { Width = SizingAxis.Fixed(800), Height = SizingAxis.Fixed(600) } 
                }, new Color(40, 44, 52), () => {
                    UI.Container("parent", new LayoutConfig { 
                        Sizing = new Sizing { Width = SizingAxis.Fixed(200), Height = SizingAxis.Fixed(200) } 
                    }, new Color(100, 100, 100), () => {
                        UI.FloatingContainer("floating", new LayoutConfig { 
                            Sizing = new Sizing { Width = SizingAxis.Fixed(100), Height = SizingAxis.Fixed(100) } 
                        }, new FloatingConfig { 
                            AttachPoints = new FloatingAttachPoints { Element = FloatingAttachPoint.LeftTop, Parent = FloatingAttachPoint.RightBottom },
                            AttachTo = FloatingAttachToElement.Parent,
                            ZIndex = 100
                        }, new Color(255, 255, 0));

                    });
                    UI.Container("sibling", new LayoutConfig { 
                        Sizing = new Sizing { Width = SizingAxis.Fixed(200), Height = SizingAxis.Fixed(200) } 
                    }, new Color(200, 200, 200));
                });
            }, 800, 600);
    }

    [Fact]
    public void TestScrollAndScissor()
    {
        AssertParity(@"
            CLAY(CLAY_ID(""root""), { 
                .layout = { .sizing = { CLAY_SIZING_FIXED(800), CLAY_SIZING_FIXED(600) } } 
            }) {
                CLAY(CLAY_ID(""scroll""), { 
                    .layout = { .sizing = { CLAY_SIZING_FIXED(200), CLAY_SIZING_FIXED(200) } },
                    .clip = { .vertical = true, .childOffset = { 0, -50 } }
                }) {
                    CLAY(CLAY_ID(""content""), { 
                        .layout = { .sizing = { CLAY_SIZING_FIXED(100), CLAY_SIZING_FIXED(400) } },
                        .backgroundColor = { 255, 0, 0, 255 }
                    }) {}
                }
            }",
            () => {
                UI.Container("root", new LayoutConfig { 
                    Sizing = new Sizing { Width = SizingAxis.Fixed(800), Height = SizingAxis.Fixed(600) } 
                }, () => {
                    UI.ScrollContainer("scroll", new LayoutConfig { 
                        Sizing = new Sizing { Width = SizingAxis.Fixed(200), Height = SizingAxis.Fixed(200) } 
                    }, new ClipConfig { Vertical = true, ChildOffset = new Vector2(0, -50) }, () => {
                        UI.Container("content", new LayoutConfig { 
                            Sizing = new Sizing { Width = SizingAxis.Fixed(100), Height = SizingAxis.Fixed(400) } 
                        }, new Color(255, 0, 0));
                    });
                });
            }, 800, 600);
    }

    [Fact]
    public void TestDemoParity()
    {
        AssertParity(@"
            CLAY(CLAY_ID(""root""), { 
                .layout = { .layoutDirection = CLAY_TOP_TO_BOTTOM, .childGap = 10, .padding = { 20, 20, 20, 20 }, .sizing = { CLAY_SIZING_FIXED(800), CLAY_SIZING_FIXED(600) } },
                .backgroundColor = { 40, 44, 52, 255 }
            }) {
                CLAY(CLAY_ID(""header""), { 
                    .layout = { .sizing = { CLAY_SIZING_GROW(0, 0), CLAY_SIZING_FIXED(60) }, .childAlignment = { .x = CLAY_ALIGN_X_CENTER, .y = CLAY_ALIGN_Y_CENTER } },
                    .backgroundColor = { 60, 64, 72, 255 }
                }) {
                    CLAY_TEXT(CLAY_STRING(""CLAY C# PORT""), CLAY_TEXT_CONFIG({ .fontSize = 32 }));
                }
                CLAY(CLAY_ID(""body""), { 
                    .layout = { .layoutDirection = CLAY_LEFT_TO_RIGHT, .childGap = 20, .sizing = { CLAY_SIZING_GROW(0, 0), CLAY_SIZING_GROW(0, 0) } }
                }) {
                    CLAY(CLAY_ID(""sidebar""), { 
                        .layout = { .sizing = { CLAY_SIZING_FIXED(200), CLAY_SIZING_GROW(0, 0) }, .padding = { 10, 10, 10, 10 }, .childGap = 10 },
                        .backgroundColor = { 50, 54, 62, 255 }
                    }) {
                        CLAY(CLAY_ID(""item-1""), { .layout = { .sizing = { CLAY_SIZING_GROW(0, 0), CLAY_SIZING_FIXED(40) } }, .backgroundColor = { 70, 74, 82, 255 } }) {}
                    }
                    CLAY(CLAY_ID(""content""), { 
                        .layout = { .sizing = { CLAY_SIZING_GROW(0, 0), CLAY_SIZING_GROW(0, 0) }, .padding = { 20, 20, 20, 20 }, .childGap = 20 },
                        .backgroundColor = { 30, 34, 42, 255 }
                    }) {
                        CLAY_TEXT(CLAY_STRING(""Welcome to the Clay C# Port!""), CLAY_TEXT_CONFIG({ .fontSize = 24 }));
                    }
                }
            }",
            () => {
                Container("root", c => c
                    .Direction(LayoutDirection.TopToBottom)
                    .ChildGap(10)
                    .Padding(20, 20)
                    .Sizing(Fixed(800), Fixed(600))
                , new Color(40, 44, 52), () => 
                {
                    Container("header", c => c
                        .Sizing(Grow(), Fixed(60))
                        .Align(LayoutAlignmentX.Center, LayoutAlignmentY.Center)
                    , new Color(60, 64, 72), () => 
                    {
                        Text("CLAY C# PORT", t => t.Size(32).Color(255, 255, 255));
                    });

                    Container("body", c => c
                        .Direction(LayoutDirection.LeftToRight)
                        .ChildGap(20)
                        .Sizing(Grow(), Grow())
                    , () => 
                    {
                        Container("sidebar", c => c
                            .Sizing(Fixed(200), Grow())
                            .Padding(10, 10)
                            .ChildGap(10)
                        , new Color(50, 54, 62), () => 
                        {
                            Container("item-1", c => c.Sizing(Grow(), Fixed(40)), new Color(70, 74, 82));
                        });

                        Container("content", c => c
                            .Sizing(Grow(), Grow())
                            .Padding(20, 20)
                            .ChildGap(20)
                        , new Color(30, 34, 42), () => 
                        {
                            Text("Welcome to the Clay C# Port!", t => t.Size(24).Color(255, 255, 255));
                        });
                    });
                });
            }, 800, 600);
    }

    [Fact]
    public void TestTextWrappingAndFit()
    {
        AssertParity(@"
            CLAY(CLAY_ID(""root""), { 
                .layout = { .sizing = { CLAY_SIZING_FIXED(200), CLAY_SIZING_FIT(0, 0) } },
                .backgroundColor = { 40, 44, 52, 255 }
            }) {
                CLAY_TEXT(CLAY_STRING(""This is a long sentence that should wrap into multiple lines based on the fixed width of the parent.""), CLAY_TEXT_CONFIG({ .fontSize = 20 }));
            }",
            () => {
                Container("root", c => c
                    .Sizing(Fixed(200), Fit())
                , new Color(40, 44, 52), () => {
                    Text("This is a long sentence that should wrap into multiple lines based on the fixed width of the parent.", t => t.Size(20));
                });
            }, 800, 600);
    }
}
