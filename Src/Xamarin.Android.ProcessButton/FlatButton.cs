using System.Collections.Generic;
using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Android.Widget;

namespace Xamarin
{
    public class FlatButton : Button
    {
        private Drawable _mNormalDrawable;
        private string _mNormalText;
        private float _cornerRadius;

        public FlatButton(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            Init(context, attrs);
        }

        public FlatButton(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Init(context, attrs);
        }

        public FlatButton(Context context)
            : base(context)
        {
            Init(context, null);
        }

        private void Init(Context context, IAttributeSet attrs)
        {
            _mNormalDrawable = Background;
            if (attrs != null)
            {
                InitAttributes(context, attrs);
            }
            _mNormalText = Text;
        }

        private void InitAttributes(Context context, IAttributeSet attributeSet)
        {
            var attr = GetTypedArray(context, attributeSet, Resource.Styleable.FlatButton);
            if (attr == null)
            {
                return;
            }

            try
            {
                var defValue = GetDimension(Resource.Dimension.corner_radius);
                _cornerRadius = attr.GetDimension(Resource.Styleable.FlatButton_pb_cornerRadius, defValue);

                var gradients = FindDravables<GradientDrawable>(_mNormalDrawable);

                foreach (var gradientDrawable in gradients)
                {
                    gradientDrawable.SetCornerRadius(_cornerRadius);
                }
            }
            finally
            {
                attr.Recycle();
            }
        }

        private IEnumerable<T> FindDravables<T>(Drawable drawable)
            where T : class 
        {
            var results = new List<T>();
            var layerDrawable = drawable as LayerDrawable;
            if (layerDrawable != null)
            {
                for (var i = 0; i < layerDrawable.NumberOfLayers; i++)
                {
                    var d = layerDrawable.GetDrawable(i) as T;
                    if (d != null)
                    {
                        results.Add(d);
                    }
                }
            }

            var stateListDrawable = drawable as DrawableContainer;
            if (stateListDrawable != null)
            {
                var state = stateListDrawable.GetConstantState() as DrawableContainer.DrawableContainerState;
                if (state != null)
                {
                    foreach (var child in state.GetChildren())
                    {
                        var d = child as T;
                        if (d != null)
                        {
                            results.Add(d);
                        } 
                    }
                    
                }
            }

            return results;
        }
        /* Przerobić to na automat */
        private LayerDrawable CreateNormalDrawable(TypedArray attr)
        {
            var drawableNormal = (LayerDrawable)GetDrawable(Resource.Drawable.rect_normal).Mutate();

            var drawableTop = (GradientDrawable)drawableNormal.GetDrawable(0).Mutate();
            drawableTop.SetCornerRadius(CornerRadius);

            var blueDark = GetColor(Resource.Color.blue_pressed);
            //var colorPressed = attr.GetColor(Resource.Styleable.FlatButton_pb_colorPressed, blueDark);
            //drawableTop.SetColor(colorPressed);

            var drawableBottom = (GradientDrawable)drawableNormal.GetDrawable(1).Mutate();
            drawableBottom.SetCornerRadius(CornerRadius);

            var blueNormal = GetColor(Resource.Color.blue_normal);
            //var colorNormal = attr.GetColor(Android.Resource.B, blueNormal);
            //drawableBottom.SetColor();
            var x = this.Background;

            return drawableNormal;
        }

        private Drawable CreatePressedDrawable(TypedArray attr)
        {
            var drawablePressed = (GradientDrawable)GetDrawable(Resource.Drawable.rect_pressed).Mutate();
            drawablePressed.SetCornerRadius(CornerRadius);

            int blueDark = GetColor(Resource.Color.blue_pressed);
            //int colorPressed = attr.GetColor(Resource.Styleable.FlatButton_pb_colorPressed, blueDark);
            //drawablePressed.SetColor(colorPressed);

            return drawablePressed;
        }

        protected virtual Drawable GetDrawable(int id)
        {
            return Resources.GetDrawable(id);
        }

        protected virtual float GetDimension(int id)
        {
            return Resources.GetDimension(id);
        }

        protected virtual int GetColor(int id)
        {
            return Resources.GetColor(id);
        }

        protected virtual TypedArray GetTypedArray(Context context, IAttributeSet attributeSet, int[] attr)
        {
            return context.ObtainStyledAttributes(attributeSet, attr, 0, 0);
        }

        public virtual float CornerRadius
        {
            get
            {
                return _cornerRadius;
            }
        }

        public virtual Drawable NormalDrawable
        {
            get
            {
                return _mNormalDrawable;
            }
        }

        public virtual string NormalText
        {
            get
            {
                return _mNormalText;
            }
        }

        /// <summary>
        /// Set the View's background. Masks the API changes made in Jelly Bean.
        /// </summary>
        /// <param name="drawable"> </param>
        public virtual void SetBackgroundCompat(Drawable drawable)
        {
            int pL = PaddingLeft;
            int pT = PaddingTop;
            int pR = PaddingRight;
            int pB = PaddingBottom;

            if (Build.VERSION.SdkInt >= (BuildVersionCodes)16/*BuildVersionCodes.JellyBean*/)
            {
                //Background = drawable;
            }
            else
            {
                SetBackgroundDrawable(drawable);
            }
            SetPadding(pL, pT, pR, pB);
        }
    }

}