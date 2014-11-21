using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;

namespace Xamarin
{
    public abstract class ProcessButton : FlatButton
    {
        private int _mProgress;
        private int _mMaxProgress;
        private int _mMinProgress;

        private GradientDrawable _mProgressDrawable;
        private GradientDrawable _mCompleteDrawable;
        private GradientDrawable _mErrorDrawable;

        private string _mLoadingText;
        private string _mCompleteText;
        private string _mErrorText;
        private bool _isBusy;

        public ProcessButton(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            init(context, attrs);
        }

        public ProcessButton(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            init(context, attrs);
        }

        public ProcessButton(Context context)
            : base(context)
        {
            init(context, null);
        }

        private void init(Context context, IAttributeSet attrs)
		{
			_mMinProgress = 0;
			_mMaxProgress = 100;

			_mProgressDrawable = (GradientDrawable) GetDrawable(Resource.Drawable.rect_progress).Mutate();
			_mProgressDrawable.SetCornerRadius(CornerRadius);

			_mCompleteDrawable = (GradientDrawable) GetDrawable(Resource.Drawable.rect_complete).Mutate();
			_mCompleteDrawable.SetCornerRadius(CornerRadius);

			_mErrorDrawable = (GradientDrawable) GetDrawable(Resource.Drawable.rect_error).Mutate();
			_mErrorDrawable.SetCornerRadius(CornerRadius);

			if (attrs != null)
			{
				initAttributes(context, attrs);
			}
		}

        private void initAttributes(Context context, IAttributeSet attributeSet)
        {
            var attr = GetTypedArray(context, attributeSet, Resource.Styleable.ProcessButton);

            if (attr == null)
            {
                return;
            }

            try
            {
                _mLoadingText = attr.GetString(Resource.Styleable.ProcessButton_pb_textProgress);
                _mCompleteText = attr.GetString(Resource.Styleable.ProcessButton_pb_textComplete);
                _mErrorText = attr.GetString(Resource.Styleable.ProcessButton_pb_textError);

                int purple = GetColor(Resource.Color.purple_progress);
                int colorProgress = attr.GetColor(Resource.Styleable.ProcessButton_pb_colorProgress, purple);
                _mProgressDrawable.SetColor(colorProgress);

                int green = GetColor(Resource.Color.green_complete);
                int colorComplete = attr.GetColor(Resource.Styleable.ProcessButton_pb_colorComplete, green);
                _mCompleteDrawable.SetColor(colorComplete);

                int red = GetColor(Resource.Color.red_error);
                int colorError = attr.GetColor(Resource.Styleable.ProcessButton_pb_colorError, red);
                _mErrorDrawable.SetColor(colorError);

            }
            finally
            {
                attr.Recycle();
            }
        }

        protected virtual void OnErrorState()
        {
            if (ErrorText != null)
            {
                Text = ErrorText;
            }

            SetBackgroundCompat(ErrorDrawable);
        }

        protected virtual void OnProgress()
        {
            if (LoadingText != null)
            {
                Text = LoadingText;
            }

            SetBackgroundCompat(NormalDrawable);
        }

        protected virtual void OnCompleteState()
        {
            if (CompleteText != null)
            {
                Text = CompleteText;
            }

            SetBackgroundCompat(CompleteDrawable);
        }

        protected virtual void OnNormalState()
        {
            if (NormalText != null)
            {
                Text = NormalText;
            }

            SetBackgroundCompat(NormalDrawable);
        }

        protected override void OnDraw(Canvas canvas)
        {
            if (IsBusy)
            {
                DrawProgress(canvas);
            }

            base.OnDraw(canvas);
        }

        public abstract void DrawProgress(Canvas canvas);


        public virtual int MaxProgress
        {
            get
            {
                return _mMaxProgress;
            }
        }

        public virtual int Progress
        {
            get
            {
                return _mProgress;
            }
            set
            {
                _mProgress = value;

                if (_mProgress == _mMinProgress)
                {
                    OnNormalState();
                }
                else if (_mProgress == _mMaxProgress)
                {
                    OnCompleteState();
                }
                else if (_mProgress < _mMinProgress)
                {
                    OnErrorState();
                }
                else
                {
                    OnProgress();
                }

                Invalidate();
            }
        }


        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;

                if (_isBusy)
                {
                    OnProgress();
                }
                else
                {
                    OnNormalState();
                }

                Invalidate();
            }
        }

        public virtual int MinProgress
        {
            get
            {
                return _mMinProgress;
            }
        }

        public virtual GradientDrawable ProgressDrawable
        {
            get
            {
                return _mProgressDrawable;
            }
            set
            {
                _mProgressDrawable = value;
            }
        }

        public virtual GradientDrawable CompleteDrawable
        {
            get
            {
                return _mCompleteDrawable;
            }
            set
            {
                _mCompleteDrawable = value;
            }
        }

        public virtual string LoadingText
        {
            get
            {
                return _mLoadingText;
            }
            set
            {
                _mLoadingText = value;
            }
        }

        public virtual string CompleteText
        {
            get
            {
                return _mCompleteText;
            }
            set
            {
                _mCompleteText = value;
            }
        }

        public virtual GradientDrawable ErrorDrawable
        {
            get
            {
                return _mErrorDrawable;
            }
            set
            {
                _mErrorDrawable = value;
            }
        }

        public virtual string ErrorText
        {
            get
            {
                return _mErrorText;
            }
            set
            {
                _mErrorText = value;
            }
        }


        public override IParcelable OnSaveInstanceState()
        {
            var superState = base.OnSaveInstanceState();
            var savedState = new SavedState(superState) { mProgress = _mProgress };

            return savedState;
        }

        public override void OnRestoreInstanceState(IParcelable state)
        {
            var savedState = state as SavedState;
            if (savedState != null)
            {
                _mProgress = savedState.mProgress;
                base.OnRestoreInstanceState(savedState.SuperState);
                Progress = _mProgress;
            }
            else
            {
                base.OnRestoreInstanceState(state);
            }
        }

        /// <summary>
        /// A <seealso cref="android.os.Parcelable"/> representing the <seealso cref="com.dd.processbutton.ProcessButton"/>'s
        /// state.
        /// </summary>
        public class SavedState : BaseSavedState
        {

            internal int mProgress;

            public SavedState(IParcelable parcel)
                : base(parcel)
            {
            }

            internal SavedState(Parcel @in)
                : base(@in)
            {
                mProgress = @in.ReadInt();
            }

            public override void WriteToParcel(Parcel parcel, ParcelableWriteFlags flags)
            {
                base.WriteToParcel(parcel, flags);

                parcel.WriteInt(mProgress);
            }

            //public static readonly IParcelableCreator Creator = new CreatorAnonymousInnerClassHelper();

            //private class CreatorAnonymousInnerClassHelper : IParcelableCreator
            //{
            //    public Object CreateFromParcel(Parcel @in)
            //    {
            //        return new SavedState(@in);
            //    }

            //    public Object[] NewArray(int size)
            //    {
            //        return new SavedState[size];
            //    }

            //    public void Dispose()
            //    {
            //    }

            //    public IntPtr Handle { get; private set; }
            //}
        }
    }

}