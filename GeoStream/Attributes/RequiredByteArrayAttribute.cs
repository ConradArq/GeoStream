using System.ComponentModel.DataAnnotations;

namespace GeoStream.Attributes
{
    public class RequiredByteArrayAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            var byteArray = value as byte[];
            if (byteArray == null || byteArray.Length == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
