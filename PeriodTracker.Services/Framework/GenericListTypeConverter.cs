using System.ComponentModel;
using System.Globalization;

namespace PeriodTracker.Services.Framework
{
    public class GenericListTypeConverter<T> : TypeConverter
    {
        private readonly TypeConverter _typeConverter;

        public GenericListTypeConverter()
        {
            _typeConverter = TypeDescriptor.GetConverter(typeof(T));
            if (_typeConverter == null)
            {
                throw new InvalidOperationException("No type converter exists for type " + typeof(T).FullName);
            }
        }

        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string text)
            {
                var result = new List<T>();
                if (string.IsNullOrWhiteSpace(text))
                {
                    return result;
                }

                var items = text.Split(',').Select(x => x.Trim());
                foreach (var item in items)
                {
                    var converted = _typeConverter.ConvertFromInvariantString(item);
                    if (converted is T typedValue)
                    {
                        result.Add(typedValue);
                    }
                }

                return result;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (value is not IList<T> list || list.Count == 0)
                {
                    return string.Empty;
                }

                return string.Join(",", list.Select(x => Convert.ToString(x, CultureInfo.InvariantCulture)));
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}