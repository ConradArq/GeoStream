using System.ComponentModel.DataAnnotations;

namespace GeoStream.Attributes
{
    public class NotEqualToZeroAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if(value == null) return false;

            Type valueType = value.GetType();

            if (!IsNumericType(valueType))
            {
                throw new InvalidOperationException("The NotEqualToZeroAttribute is not valid on non-numeric types.");
            }

            if (Convert.ToDouble(value) == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        private bool IsNumericType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
    }
}
