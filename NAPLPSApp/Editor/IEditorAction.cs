// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSApp.Editor;

public interface IEditorAction
{
    void Execute(NaplpsFormat format);
    void Undo(NaplpsFormat format);
}
