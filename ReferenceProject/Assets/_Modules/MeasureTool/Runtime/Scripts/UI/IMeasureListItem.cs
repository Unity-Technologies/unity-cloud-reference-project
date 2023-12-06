namespace Unity.ReferenceProject.MeasureTool
{
    public interface IMeasureListItem
    {
        string Id { get; set; }
        MeasureLineData Data { get; }
        bool IsShown { get; }

        void Select(bool value);
        void Show(bool value);
    }
}
