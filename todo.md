# Clay C# Port Plan

## Overview
Port the `nicbarker/clay` high-performance 2D UI layout library from C to pure, managed C# without using unmanaged blocks (`unsafe`, pointers). The library will leverage modern C# features to achieve zero-allocation during the core layout loop.

## Design Decisions
- **No Unmanaged Code:** Strictly safe code, avoiding raw pointers. 
- **Memory Model:** **Custom Arena (byte[] + Span)**. We will pre-allocate managed `byte[]` arrays and create typed `Span<T>` / `Memory<T>` views over them to replicate the static C arena. This avoids GC pressure while maintaining safe bounds checking.
- **Declarative Syntax:** **Action Delegates (Lambdas)**. E.g., `Clay.Container(config, () => { ... })`. This gives a clean, hierarchical view of the UI tree, similar to modern frameworks like SwiftUI or Blazor, though care will be needed to avoid closure allocations in hot paths.

## Implementation Phases

### Phase 1: Core Types & Memory Arena
- [x] Define basic structs: `Dimensions`, `BoundingBox`, `Vector2`, `Color`.
- [x] Define Enums: `LayoutDirection`, `LayoutAlignment`, `SizingRule`, etc.
- [x] Define configuration structs: `LayoutConfig`, `TextConfig`, `ImageConfig`.
- [x] Implement the `ClayArena`: A pre-allocated memory pool backed by `byte[]`.
- [x] Create generic methods to slice `Span<T>` from the `ClayArena` for core arrays (`Clay_ElementData`, `Clay_RenderCommand`, etc.).

### Phase 2: State & ID Generation
- [x] Port `Clay_Context` to manage the global or thread-local layout state.
- [x] Implement the string hashing algorithm (FNV-1a) for tracking Element IDs across frames using `ReadOnlySpan<char>` to avoid string allocations.
- [x] Implement internal state tracking for scroll positions, pointer interactions, and hover states.

### Phase 3: Declarative Builder API
- [x] Implement `Clay.BeginLayout(Arena arena, Dimensions windowSize)` and `Clay.EndLayout()`.
- [x] Implement lambda-based builder methods:
  - `Clay.UI(Action children)`
  - `Clay.Container(LayoutConfig config, Action children)`
  - `Clay.ScrollContainer(ScrollConfig config, Action children)`
  - `Clay.Text(string text, TextConfig config)`
  - `Clay.Image(ImageConfig config)`
  - `Clay.Custom(CustomConfig config)`
- [x] Ensure that internal elements are successfully appended to the arena during these calls.

### Phase 4: Layout Engine (The Heavy Lifting)
- [x] Implement the text measurement callback interface/delegate.
- [x] Port the multi-pass layout algorithm:
  - [x] Pass 1: Measure dimensions (Basic X/Y axis sizing).
  - [x] Pass 2: Distribute flex space / Grow.
  - [x] Pass 3: Apply absolute positioning and calculate final `BoundingBox`es.
- [x] Handle scroll boundaries and clipping rects.

### Phase 5: Render Commands & Output
- [x] Translate `Clay_ElementData` into a flat array of `RenderCommand` structs.
- [x] Ensure the output array accurately represents overlapping z-indexes and clipping regions (`Scissor` commands).
- [x] Validate that the command generation accesses the arena correctly and produces 0 GC allocations.

### Phase 6: Validation & Testing
- [x] Write unit tests to verify the layout engine outputs the correct coordinates for a given configuration.
- [x] Implement a simple test backend (Raylib-cs demo created in `examples/Clay.Demo`).
- [ ] Benchmark against the C library to ensure performance remains microsecond-fast.

