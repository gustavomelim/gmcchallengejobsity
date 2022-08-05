using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System;
using System.Globalization;

namespace JobsityNetChallenge.Domain.Utils
{
    public class CustomDecimalConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(string data, IReaderRow row, MemberMapData memberMapData)
        {
            decimal result = -1;
            if (!string.IsNullOrWhiteSpace(data))
            {
                decimal.TryParse(data, NumberStyles.Float, LocalizationUtil.GLOBAL_CULTURE, out result);
            }
            return result;
        }
    }

    public class CustomLongConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(string data, IReaderRow row, MemberMapData memberMapData)
        {
            long result = -1;
            if (!string.IsNullOrWhiteSpace(data))
            {
                long.TryParse(data, NumberStyles.Float, LocalizationUtil.GLOBAL_CULTURE, out result);
            }
            return result;
        }
    }

    public class CustomDateConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(string data, IReaderRow row, MemberMapData memberMapData)
        {
            DateTime result;
            if (DateTime.TryParseExact(data, "yyyy-MM-dd", LocalizationUtil.GLOBAL_CULTURE, LocalizationUtil.GLOBAL_DATE_STYLE, out result))
            {
                return result;
            }
            return DateTime.MinValue;
        }
    }

    public class CustomTimeConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(string data, IReaderRow row, MemberMapData memberMapData)
        {
            DateTime result;
            if (DateTime.TryParseExact(data, "HH:mm:ss", LocalizationUtil.GLOBAL_CULTURE, LocalizationUtil.GLOBAL_DATE_STYLE, out result))
            {
                return result;
            }
            return DateTime.MinValue;
        }
    }

    public class StockInfoWithConverter : ClassMap<StockInfo>
    {
        public StockInfoWithConverter()
        {
            Map(p => p.Symbol).TypeConverter<StringConverter>();
            Map(p => p.High).TypeConverter<CustomDecimalConverter>();
            Map(p => p.Low).TypeConverter<CustomDecimalConverter>();
            Map(p => p.Open).TypeConverter<CustomDecimalConverter>();
            Map(p => p.Close).TypeConverter<CustomDecimalConverter>();
            Map(p => p.Volume).TypeConverter<CustomLongConverter>();
            Map(p => p.Date).TypeConverter<CustomDateConverter>();
            Map(p => p.Time).TypeConverter<CustomTimeConverter>();
        }
    }
}
