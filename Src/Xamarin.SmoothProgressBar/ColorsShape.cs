using Android.Graphics;
using Android.Graphics.Drawables.Shapes;

namespace Xamarin
{
    public class ColorsShape : Shape
    {

        private float _strokeWidth;
        private int[] _colors;

        public ColorsShape(float strokeWidth, int[] colors)
        {
            _strokeWidth = strokeWidth;
            _colors = colors;
        }

        public virtual float StrokeWidth
        {
            get
            {
                return _strokeWidth;
            }
            set
            {
                _strokeWidth = value;
            }
        }


        public virtual int[] Colors
        {
            get
            {
                return _colors;
            }
            set
            {
                _colors = value;
            }
        }

        public override void Draw(Canvas canvas, Paint paint)
        {
            var ratio = 1f / _colors.Length;
            var i = 0;
            paint.StrokeWidth = _strokeWidth;
            foreach (var color in _colors)
            {
                paint.Color = new Color(color);
                canvas.DrawLine(i * ratio * Width, Height / 2, ++i * ratio * Width, Height / 2, paint);
            }
        }
    }

}