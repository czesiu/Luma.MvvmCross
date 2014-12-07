using System;
using System.Linq;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cirrious.CrossCore.Platform;

namespace Luma.MvvmCross
{
    public class DataFormDataField : LinearLayout
    {
        private View _editView;
        private object _value;
        private readonly TextView _labelTextView;
        private string _label;
        private readonly int _originalChildCount;
        private readonly ViewGroup _contentViewGroup;

        public event EventHandler ValueChanged;

        public string Label
        {
            get { return _label; }
            set
            {
                if (_label == value)
                    return;

                _label = value;

                if (_labelTextView == null)
                    return;

                _labelTextView.Text = _label;
            }
        }

        public object Value
        {
            get { return _value; }
            set
            {
                if (Equals(_value, value))
                    return;

                _value = value;
                UpdateValue();
                RaiseValueChanged();
            }
        }

        protected DataFormDataField(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer) { }
        public DataFormDataField(Context context)
            : this(context, null) { }
        public DataFormDataField(Context context, IAttributeSet attrs)
            : this(context, attrs, 0) { }

        public DataFormDataField(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            var attr = context.ObtainStyledAttributes(attrs, Resource.Styleable.DataFormDataField, defStyle, 0);
            var standartAttributes = context.ObtainStyledAttributes(attrs, new[] { Android.Resource.Attribute.Text });
            if (attr == null)
            {
                return;
            }

            try
            {
                Label = attr.GetString(Resource.Styleable.DataFormDataField_label) ?? standartAttributes.GetString(0);
            }
            finally
            {
                attr.Recycle();
            }

            Inflate(Context, Resource.Layout.DataFormDataField, this);

            _labelTextView = FindViewById<TextView>(Resource.Id.DataFormLabel);
            _labelTextView.Text = Label;

            _contentViewGroup = FindViewById<ViewGroup>(Resource.Id.DataFormContent);

            _originalChildCount = ChildCount;
        }

        protected override void OnFinishInflate()
        {
            // Collect children declared in XML.
            var children = new View[ChildCount - _originalChildCount];
            for (var i = 0; i < children.Length; i++)
            {
                children[i] = GetChildAt(_originalChildCount + i);
            }

            foreach (var child in children)
            {
                DetachViewFromParent(child);
                _contentViewGroup.AddView(child);
            }

            if (_contentViewGroup.ChildCount == 0)
            {
                _editView = CreateControl();
                UpdateValue();
                _contentViewGroup.AddView(_editView);
            }

            base.OnFinishInflate();
        }

        protected virtual void UpdateValue()
        {
            var editText = _editView as EditText;
            var value = (Value ?? "").ToString();
            if (editText != null && editText.Text != value)
            {
                editText.Text = value;
            }
        }

        protected virtual View CreateControl()
        {
            var editText = new EditText(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent)
            };

            editText.TextChanged += (sender, args) => Value = args.Text.ToString();

            return editText;
        }

        private void RaiseValueChanged()
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, EventArgs.Empty);
            }
        }
    }
}