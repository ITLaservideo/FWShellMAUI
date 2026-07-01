namespace FWShellMAUI.Controls {
    internal sealed class SpinnerDrawable : IDrawable {
        public void Draw(ICanvas canvas, RectF dirtyRect) {
            const float strokeWidth = 4f;
            var rect = dirtyRect;
            rect.Inflate(-strokeWidth / 2, -strokeWidth / 2);

            canvas.StrokeColor = Colors.White;
            canvas.StrokeSize = strokeWidth;
            canvas.StrokeLineCap = LineCap.Round;
            canvas.DrawArc(rect.X, rect.Y, rect.Width, rect.Height, 90, -180, true, false);
        }
    }
}
