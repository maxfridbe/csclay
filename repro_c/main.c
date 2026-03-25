#define CLAY_IMPLEMENTATION
#include "../reference/clay.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

Clay_Dimensions MockTextMeasure(Clay_StringSlice text, Clay_TextElementConfig *config, void *userData) {
    float w = text.length * (config->fontSize * 0.55f);
    return (Clay_Dimensions) { .width = w, .height = config->fontSize * 1.2f };
}

void FeatureCard(char* title, char* desc) {
    Clay_String titleStr = { .length = strlen(title), .chars = title };
    Clay_String descStr = { .length = strlen(desc), .chars = desc };
    
    Clay__OpenElementWithId(Clay__HashString(titleStr, 0));
    Clay__ConfigureOpenElement((Clay_ElementDeclaration) {
        .layout = { 
            .layoutDirection = CLAY_TOP_TO_BOTTOM, 
            .padding = {24, 24, 24, 24}, 
            .childGap = 12,
            .sizing = { CLAY_SIZING_GROW(0, 0), CLAY_SIZING_FIT(0, 0) }
        },
        .backgroundColor = {50, 54, 62, 255}
    });
    CLAY_TEXT(titleStr, CLAY_TEXT_CONFIG({ .fontSize = 20, .textColor = {100, 150, 255, 255} }));
    CLAY_TEXT(descStr, CLAY_TEXT_CONFIG({ .fontSize = 16, .textColor = {180, 185, 190, 255}, .wrapMode = CLAY_TEXT_WRAP_WORDS }));
    Clay__CloseElement();
}

void CreateLayout(int width, int height) {
    bool isMobile = width < 600;
    
    Clay__OpenElementWithId(CLAY_ID("root"));
    Clay__ConfigureOpenElement((Clay_ElementDeclaration) {
        .layout = { 
            .layoutDirection = CLAY_TOP_TO_BOTTOM, 
            .sizing = { CLAY_SIZING_FIXED(width), CLAY_SIZING_FIXED(height) }
        },
        .backgroundColor = {40, 44, 52, 255}
    });

    // HEADER
    Clay__OpenElementWithId(CLAY_ID("header"));
    Clay__ConfigureOpenElement((Clay_ElementDeclaration) {
        .layout = { 
            .sizing = { CLAY_SIZING_GROW(0, 0), CLAY_SIZING_FIXED(80) },
            .padding = {40, 40, 0, 0},
            .layoutDirection = CLAY_LEFT_TO_RIGHT,
            .childAlignment = { .y = CLAY_ALIGN_Y_CENTER },
            .childGap = 20
        },
        .backgroundColor = {28, 32, 38, 255}
    });
    CLAY_TEXT(CLAY_STRING("CLAY C#"), CLAY_TEXT_CONFIG({ .fontSize = 28, .textColor = {100, 150, 255, 255} }));
    if (!isMobile) {
        Clay__OpenElementWithId(CLAY_ID("nav"));
        Clay__ConfigureOpenElement((Clay_ElementDeclaration) {
            .layout = { 
                .layoutDirection = CLAY_LEFT_TO_RIGHT, 
                .childGap = 30,
                .sizing = { CLAY_SIZING_GROW(0, 0), CLAY_SIZING_FIT(0, 0) },
                .childAlignment = { .x = CLAY_ALIGN_X_RIGHT, .y = CLAY_ALIGN_Y_CENTER }
            }
        });
        CLAY_TEXT(CLAY_STRING("Documentation"), CLAY_TEXT_CONFIG({ .fontSize = 16, .textColor = {255, 255, 255, 255} }));
        CLAY_TEXT(CLAY_STRING("Examples"), CLAY_TEXT_CONFIG({ .fontSize = 16, .textColor = {255, 255, 255, 255} }));
        CLAY_TEXT(CLAY_STRING("GitHub"), CLAY_TEXT_CONFIG({ .fontSize = 16, .textColor = {255, 255, 255, 255} }));
        Clay__CloseElement();
    }
    Clay__CloseElement(); // header

    // HERO
    Clay__OpenElementWithId(CLAY_ID("hero"));
    Clay__ConfigureOpenElement((Clay_ElementDeclaration) {
        .layout = { 
            .layoutDirection = isMobile ? CLAY_TOP_TO_BOTTOM : CLAY_LEFT_TO_RIGHT,
            .padding = {60, 60, 60, 60},
            .childGap = 40,
            .sizing = { CLAY_SIZING_GROW(0, 0), CLAY_SIZING_FIT(0, 0) }
        }
    });

    Clay__OpenElementWithId(CLAY_ID("hero-text"));
    Clay__ConfigureOpenElement((Clay_ElementDeclaration) {
        .layout = { 
            .layoutDirection = CLAY_TOP_TO_BOTTOM,
            .childGap = 20,
            .sizing = { isMobile ? CLAY_SIZING_GROW(0, 0) : CLAY_SIZING_PERCENT(0.6f), CLAY_SIZING_FIT(0, 0) }
        }
    });
    CLAY_TEXT(CLAY_STRING("High performance 2D UI layout library in pure C#."), 
        CLAY_TEXT_CONFIG({ .fontSize = 48, .textColor = {255, 255, 255, 255}, .wrapMode = CLAY_TEXT_WRAP_WORDS }));
    CLAY_TEXT(CLAY_STRING("csclay provides a microsecond layout engine with zero GC allocations in the core loop. It's safe, managed, and renderer-agnostic."), 
        CLAY_TEXT_CONFIG({ .fontSize = 20, .textColor = {180, 185, 190, 255}, .wrapMode = CLAY_TEXT_WRAP_WORDS }));
    
    Clay__OpenElementWithId(CLAY_ID("cta-row"));
    Clay__ConfigureOpenElement((Clay_ElementDeclaration) {
        .layout = { .layoutDirection = CLAY_LEFT_TO_RIGHT, .childGap = 16 }
    });
    
    Clay__OpenElementWithId(CLAY_ID("btn-primary"));
    Clay__ConfigureOpenElement((Clay_ElementDeclaration) {
        .layout = { .padding = {24, 24, 12, 12} },
        .backgroundColor = {100, 150, 255, 255}
    });
    CLAY_TEXT(CLAY_STRING("Get Started"), CLAY_TEXT_CONFIG({ .fontSize = 18, .textColor = {40, 44, 52, 255} }));
    Clay__CloseElement();

    Clay__OpenElementWithId(CLAY_ID("btn-secondary"));
    Clay__ConfigureOpenElement((Clay_ElementDeclaration) {
        .layout = { .padding = {24, 24, 12, 12} },
        .backgroundColor = {60, 64, 72, 255}
    });
    CLAY_TEXT(CLAY_STRING("View Source"), CLAY_TEXT_CONFIG({ .fontSize = 18, .textColor = {255, 255, 255, 255} }));
    Clay__CloseElement();

    Clay__CloseElement(); // cta-row
    Clay__CloseElement(); // hero-text

    Clay__OpenElementWithId(CLAY_ID("hero-graphic"));
    Clay__ConfigureOpenElement((Clay_ElementDeclaration) {
        .layout = { .sizing = { isMobile ? CLAY_SIZING_GROW(0, 0) : CLAY_SIZING_PERCENT(0.4f), CLAY_SIZING_FIXED(isMobile ? 200 : 300) } },
        .backgroundColor = {50, 54, 62, 255}
    });
    Clay__OpenElementWithId(CLAY_ID("box"));
    Clay__ConfigureOpenElement((Clay_ElementDeclaration) {
        .layout = { .sizing = { CLAY_SIZING_GROW(0, 0), CLAY_SIZING_GROW(0, 0) }, .padding = {40, 40, 40, 40} }
    });
    Clay__OpenElementWithId(CLAY_ID("inner-graphic"));
    Clay__ConfigureOpenElement((Clay_ElementDeclaration) {
        .layout = { .sizing = { CLAY_SIZING_GROW(0, 0), CLAY_SIZING_GROW(0, 0) } },
        .backgroundColor = {100, 150, 255, 255}
    });
    Clay__CloseElement();
    Clay__CloseElement();
    Clay__CloseElement(); // hero-graphic

    Clay__CloseElement(); // hero

    // FEATURES
    Clay__OpenElementWithId(CLAY_ID("features"));
    Clay__ConfigureOpenElement((Clay_ElementDeclaration) {
        .layout = { 
            .layoutDirection = CLAY_TOP_TO_BOTTOM,
            .padding = {20, 20, 60, 60},
            .childGap = 30,
            .sizing = { CLAY_SIZING_GROW(0, 0), CLAY_SIZING_GROW(0, 0) }
        }
    });
    CLAY_TEXT(CLAY_STRING("Features"), CLAY_TEXT_CONFIG({ .fontSize = 32, .textColor = {255, 255, 255, 255} }));
    
    Clay__OpenElementWithId(CLAY_ID("grid"));
    Clay__ConfigureOpenElement((Clay_ElementDeclaration) {
        .layout = { 
            .layoutDirection = isMobile ? CLAY_TOP_TO_BOTTOM : CLAY_LEFT_TO_RIGHT,
            .childGap = 20,
            .sizing = { CLAY_SIZING_GROW(0, 0), CLAY_SIZING_FIT(0, 0) }
        }
    });
    FeatureCard("Zero GC", "No heap allocations in the hot path.");
    FeatureCard("Responsive", "Easy desktop and mobile layouts.");
    FeatureCard("Fast", "Microsecond layout calculations.");
    Clay__CloseElement(); // grid
    Clay__CloseElement(); // features

    Clay__CloseElement(); // root
}

const char* GetCommandTypeName(int type) {
    switch(type) {
        case 0: return "NONE";
        case 1: return "RECTANGLE";
        case 2: return "BORDER";
        case 3: return "TEXT";
        case 4: return "IMAGE";
        case 5: return "SCISSOR_START";
        case 6: return "SCISSOR_END";
        case 7: return "CUSTOM";
        default: return "UNKNOWN";
    }
}

int main() {
    uint64_t arenaSize = Clay_MinMemorySize();
    Clay_Arena arena = Clay_CreateArenaWithCapacityAndMemory(arenaSize, malloc(arenaSize));
    Clay_Initialize(arena, (Clay_Dimensions) { 1200, 800 }, (Clay_ErrorHandler) { 0 });
    Clay_SetMeasureTextFunction(MockTextMeasure, NULL);

    Clay_BeginLayout();
    CreateLayout(1200, 800);
    Clay_RenderCommandArray commands = Clay_EndLayout();

    for (int i = 0; i < commands.length; i++) {
        Clay_RenderCommand *cmd = &commands.internalArray[i];
        printf("CMD: %s, X: %.1f, Y: %.1f, W: %.1f, H: %.1f\n", 
            GetCommandTypeName(cmd->commandType), cmd->boundingBox.x, cmd->boundingBox.y, cmd->boundingBox.width, cmd->boundingBox.height);
    }

    return 0;
}
