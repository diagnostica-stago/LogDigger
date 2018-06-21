using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using LogDigger.Gui.ViewModels.LogEntries;

namespace LogDigger.Gui.ViewModels.Pages.Replay
{
    public class LogPlayer
    {
        public async Task Play(Process process, LogEntryVm logEntry, IList<LogEntryVm> entries)
        {
            var inputLogInfo = logEntry.ContentInfo as InputContentInfo;
            if (inputLogInfo != null)
            {
                Prepare(logEntry, inputLogInfo, entries);
                await Play(process, logEntry, inputLogInfo);
            }
        }

        private void Prepare(LogEntryVm logEntry, InputContentInfo inputLogInfo, IList<LogEntryVm> entries)
        {
            if (inputLogInfo.Type != "PreviewKeyDown" || inputLogInfo.Data != "V")
            {
                return;
            }
            var index = entries.IndexOf(logEntry);
            if (index <= 0)
            {
                return;
            }
            var previousEntry = entries[index - 1];
            var previousInputContentInfo = previousEntry.ContentInfo as InputContentInfo;
            if (previousInputContentInfo == null)
            {
                return;
            }
            if (previousInputContentInfo.Data != "LeftCtrl" && previousInputContentInfo.Data != "RightCtrl")
            {
                return;
            }
            // its a copy paste, find the pasting info
            if (index > entries.Count - 2)
            {
                return;
            }
            var nextEntry = entries[index + 1];
            var nextInputContentInfo = nextEntry.ContentInfo as InputContentInfo;
            if (nextInputContentInfo == null)
            {
                return;
            }
            if (nextInputContentInfo.Type != "Pasting")
            {
                return;
            }
            var pasteData = nextInputContentInfo.Data.Replace(@"\n", Environment.NewLine);
            Clipboard.SetText(pasteData);
        }

        private async Task Play(Process process, LogEntryVm logEntry, InputContentInfo inputLogInfo)
        {
            var windowTitle = inputLogInfo.Window;
            var isForeground = NativeMethods.BringToFront(process, windowTitle);
            if (!isForeground)
            {
                Debug.WriteLine($"Cannot bring {process.ProcessName} to foreground");
                return;
            }
            Microsoft.Test.Input.Key key;
            if (Enum.TryParse(inputLogInfo.Data, out key))
            {
                switch (inputLogInfo.Type)
                {
                    case "PreviewKeyDown":
                        Microsoft.Test.Input.Keyboard.Press(key);
                        break;
                    case "PreviewKeyUp":
                        Microsoft.Test.Input.Keyboard.Release(key);
                        break;
                    default:
                        break;
                }
            }
            Point? position = null;
            var positions = inputLogInfo.Data.Split(';');
            if (positions.Length == 2)
            {
                double x, y;
                if (double.TryParse(positions[0], out x) && double.TryParse(positions[1], out y))
                {
                    position = new Point(x, y);
                }
            }
            if (position.HasValue)
            {
                var pointOnScreen = GetPointToScreen(process, position, windowTitle);
                if (pointOnScreen.HasValue)
                {
                    switch (inputLogInfo.Type)
                    {
                        case "PreviewMouseUp":
                            Microsoft.Test.Input.Mouse.MoveTo(new System.Drawing.Point((int) pointOnScreen.Value.X,
                                (int) pointOnScreen.Value.Y));
                            await Task.Delay(TimeSpan.FromMilliseconds(5));
                            Microsoft.Test.Input.Mouse.Up(Microsoft.Test.Input.MouseButton.Left);
                            break;
                        case "PreviewMouseMove":
                            Microsoft.Test.Input.Mouse.MoveTo(new System.Drawing.Point((int) pointOnScreen.Value.X,
                                (int) pointOnScreen.Value.Y));
                            break;
                        case "PreviewMouseDown":
                            Microsoft.Test.Input.Mouse.MoveTo(new System.Drawing.Point((int) pointOnScreen.Value.X,
                                (int) pointOnScreen.Value.Y));
                            await Task.Delay(TimeSpan.FromMilliseconds(5));
                            Microsoft.Test.Input.Mouse.Down(Microsoft.Test.Input.MouseButton.Left);
                            break;
                        case "PreviewTouchUp":
                            TouchUtils.TouchUp(pointOnScreen.Value);
                            break;
                        case "PreviewTouchMove":
                            TouchUtils.TouchMove(pointOnScreen.Value);
                            break;
                        case "PreviewTouchDown":
                            TouchUtils.TouchDown(pointOnScreen.Value);
                            break;
                        case "SizeChanged":
                            NativeMethods.SetWindowSize(process, windowTitle, (int) position.Value.X,
                                (int) position.Value.Y);
                            break;
                        default:
                            break;
                    }
                }
            }

            if (inputLogInfo.Type == "PreviewMouseWheel")
            {
                int? scrollDistance;
                ParseMouseWheelInfo(inputLogInfo, out scrollDistance, out position);
                var wheelPointOnScreen = GetPointToScreen(process, position, windowTitle);
                if (position.HasValue && wheelPointOnScreen.HasValue)
                {
                    Microsoft.Test.Input.Mouse.MoveTo(new System.Drawing.Point((int)wheelPointOnScreen.Value.X, (int)wheelPointOnScreen.Value.Y));
                }
                if (scrollDistance.HasValue)
                {
                    Microsoft.Test.Input.Mouse.Scroll(scrollDistance.Value / 120);
                }
            }
        }

        private void ParseMouseWheelInfo(InputContentInfo inputLogInfo, out int? scrollDelta, out Point? position)
        {
            int parsedDelta;
            position = null;
            var strDelta = string.Empty;
            var strPositionGroup = string.Empty;
            var match = Regex.Match(inputLogInfo.Data, "delta:(-?[0-9]*), pos:(.*)");
            if (match.Success)
            {
                strDelta = match.Groups[1].Value;
                strPositionGroup = match.Groups[2].Value;
            }
            else
            {
                // compat with previous log format
                strDelta = inputLogInfo.Data;
            }
            scrollDelta = null;
            if (int.TryParse(strDelta, out parsedDelta))
            {
                scrollDelta = parsedDelta;
            }
            var strPositions = strPositionGroup.Split(';');
            if (strPositions.Length == 2)
            {
                double x, y;
                if (double.TryParse(strPositions[0], out x) && double.TryParse(strPositions[1], out y))
                {
                    position = new Point(x, y);
                }
            }
        }

        private static Point? GetPointToScreen(Process process, Point? position, string window)
        {
            // var location = NativeMethods.GetAbsoluteClientRect(process, window);
            // if (!location.HasValue)
            // {
            //     return null;
            // }

            // var pointOnScreen = position.Value + new Vector(location.Value.X, location.Value.Y);
            var pointOnScreen = NativeMethods.GetClientToScreen(process, window, position.Value);
            return new Point(pointOnScreen.Value.X, pointOnScreen.Value.Y);
        }
    }
}