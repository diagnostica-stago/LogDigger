using System;
using LogDigger.Gui.ViewModels.LogEntries;

namespace LogDigger.Gui.ViewModels.Pages.Replay
{
    public static class InputUtils
    {
        public static bool IsPromotedEventOf(this LogEntryVm source, LogEntryVm target)
        {
            var srcContentInfo = source.ContentInfo as InputContentInfo;
            var targetContentInfo = target.ContentInfo as InputContentInfo;
            if (targetContentInfo == null || srcContentInfo == null)
            {
                return false;
            }
            if (source.Date - target.Date < TimeSpan.FromMilliseconds(2) 
                && (srcContentInfo.Type == "PreviewMouseUp" && targetContentInfo.Type == "PreviewTouchUp"
                    || srcContentInfo.Type == "PreviewMouseDown" && targetContentInfo.Type == "PreviewTouchDown"
                    || srcContentInfo.Type == "PreviewMouseMove" && targetContentInfo.Type == "PreviewTouchMove"))
            {
                return true;
            }
            return false;
        }
    }
}