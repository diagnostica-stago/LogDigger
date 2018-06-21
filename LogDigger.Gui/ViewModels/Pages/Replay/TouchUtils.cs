using System.Windows;
using TCD.System.TouchInjection;

namespace LogDigger.Gui.ViewModels.Pages.Replay
{
    public static class TouchUtils
    {
        public static void TouchMove(Point pointOnScreen)
        {
            var contact = MakePointerTouchInfo((int)pointOnScreen.X, (int)pointOnScreen.Y, 2, 1);
            contact.PointerInfo.PointerFlags = PointerFlags.UPDATE | PointerFlags.INRANGE | PointerFlags.INCONTACT;
            TouchInjector.InjectTouchInput(1, new[] { contact });
        }

        public static void TouchUp(Point pointOnScreen)
        {
            var contact = MakePointerTouchInfo((int)pointOnScreen.X, (int)pointOnScreen.Y, 2, 1);
            contact.PointerInfo.PointerFlags = PointerFlags.UP;
            TouchInjector.InjectTouchInput(1, new[] { contact });
        }

        public static void TouchDown(Point pointOnScreen)
        {
            var contact = MakePointerTouchInfo((int)pointOnScreen.X, (int)pointOnScreen.Y, 2, 1);
            TouchInjector.InjectTouchInput(1, new[] { contact });
        }

        public static PointerTouchInfo MakePointerTouchInfo(int x, int y, int radius, uint id, uint orientation = 90, uint pressure = 32000)
        {
            PointerTouchInfo contact = new PointerTouchInfo();
            contact.PointerInfo.pointerType = PointerInputType.TOUCH;
            contact.TouchFlags = TouchFlags.NONE;
            contact.Orientation = orientation;
            contact.Pressure = pressure;
            contact.PointerInfo.PointerFlags = PointerFlags.DOWN | PointerFlags.INRANGE | PointerFlags.INCONTACT;
            contact.TouchMasks = TouchMask.CONTACTAREA | TouchMask.ORIENTATION | TouchMask.PRESSURE;
            contact.PointerInfo.PtPixelLocation.X = x;
            contact.PointerInfo.PtPixelLocation.Y = y;
            contact.PointerInfo.PointerId = id;
            contact.ContactArea.left = x - radius;
            contact.ContactArea.right = x + radius;
            contact.ContactArea.top = y - radius;
            contact.ContactArea.bottom = y + radius;
            return contact;
        }
    }
}