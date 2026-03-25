#define CLAY_IMPLEMENTATION
#include "../reference/clay.h"
#include <stdio.h>
#include <stdlib.h>

Clay_Dimensions MockTextMeasure(Clay_StringSlice text, Clay_TextElementConfig *config, void *userData) {
    return (Clay_Dimensions) { .width = text.length * 10, .height = 20 };
}

int main() {
    uint64_t arenaSize = Clay_MinMemorySize();
    Clay_Arena arena = Clay_CreateArenaWithCapacityAndMemory(arenaSize, malloc(arenaSize));
    Clay_Initialize(arena, (Clay_Dimensions) { 800, 600 }, (Clay_ErrorHandler) { 0 });
    Clay_SetMeasureTextFunction(MockTextMeasure, NULL);

    Clay_BeginLayout();
    CLAY(CLAY_ID("root"), { 
        .layout = { .sizing = { CLAY_SIZING_FIXED(800), CLAY_SIZING_FIXED(600) } },
        .backgroundColor = { 40, 44, 52, 255 }
    }) {
        CLAY(CLAY_ID("child"), { 
            .layout = { .sizing = { CLAY_SIZING_GROW(0, 0), CLAY_SIZING_FIXED(100) } },
            .backgroundColor = { 255, 0, 0, 255 }
        }) {}
    }
    Clay_RenderCommandArray commands = Clay_EndLayout();

    for (int i = 0; i < commands.length; i++) {
        Clay_RenderCommand *cmd = &commands.internalArray[i];
        printf("Type: %d, BB: {%.1f, %.1f, %.1f, %.1f}\n", 
            cmd->commandType, cmd->boundingBox.x, cmd->boundingBox.y, cmd->boundingBox.width, cmd->boundingBox.height);
    }

    return 0;
}
