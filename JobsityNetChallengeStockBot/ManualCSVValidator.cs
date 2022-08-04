using JobsityNetChallenge.Domain.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsityNetChallenge.StockBot
{
    public static class ManualCSVValidator
    {
        public static DateTime ParseStockDate(string data)
        {
            DateTime result;
            if (DateTime.TryParseExact(data, "yyyy-MM-dd", LocalizationUtil.GLOBAL_CULTURE, LocalizationUtil.GLOBAL_DATE_STYLE, out result))
            {
                return result;
            }
            return DateTime.MinValue;
        }

        public static DateTime ParseStockTime(string data)
        {
            DateTime result;
            if (DateTime.TryParseExact(data, "HH:mm:ss", LocalizationUtil.GLOBAL_CULTURE, LocalizationUtil.GLOBAL_DATE_STYLE, out result))
            {
                return result;
            }
            return DateTime.MinValue;
        }

        public static decimal ParseStockValue(string data)
        {
            decimal result = -1;
            if (!string.IsNullOrWhiteSpace(data))
            {
                decimal.TryParse(data, NumberStyles.Float, LocalizationUtil.GLOBAL_CULTURE, out result);
            }
            return result;
        }

        public static long ParseStockVolume(string data)
        {
            int result = -1;
            if (!string.IsNullOrWhiteSpace(data))
            {
                int.TryParse(data, NumberStyles.Float, LocalizationUtil.GLOBAL_CULTURE, out result);
            }
            return result;
        }

    }
}
