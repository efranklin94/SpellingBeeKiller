using System.Text;

namespace SharedTools.Tools;

public static class Extentions
{
    public static string FixPersian(this string value)
    {
        // fix ی in persian
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < value.Length; i++)
        {
            if (Convert.ToInt32(value[i]) == 1610)
            {
                sb.Append(Char.ConvertFromUtf32(1740));
            }
            else
            {
                sb.Append(value[i]);
            }
        }

        return sb.ToString();
    }
}
