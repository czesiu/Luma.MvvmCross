using System;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Droid.Target;

namespace Luma.MvvmCross
{
    public class DataFormDataFieldValueTargetBinding : MvxAndroidTargetBinding
    {
        protected DataFormDataField DataFormDataField
        {
            get { return (DataFormDataField)Target; }
        }

        private bool _subscribed;

        public DataFormDataFieldValueTargetBinding(DataFormDataField dataFormDataField)
            : base(dataFormDataField) { }

        private void OnValueChanged(object sender, EventArgs e)
        {
            var dataFormDataField = DataFormDataField;
            if (dataFormDataField == null)
                return;

            FireValueChanged(dataFormDataField.Value);
        }

        protected override void SetValueImpl(object target, object value)
        {
            var dataFormDataField = DataFormDataField;
            if (dataFormDataField == null)
                return;

            dataFormDataField.Value = value;
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.TwoWay; }
        }

        public override void SubscribeToEvents()
        {
            var dataFormDataField = DataFormDataField;
            if (dataFormDataField == null)
                return;

            dataFormDataField.ValueChanged += OnValueChanged;
            _subscribed = true;
        }

        public override Type TargetType
        {
            get { return typeof(object); }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                var dataFormDataField = DataFormDataField;
                if (dataFormDataField != null && _subscribed)
                {
                    dataFormDataField.ValueChanged -= OnValueChanged;
                    _subscribed = false;
                }
            }
            base.Dispose(isDisposing);
        }
    }
}