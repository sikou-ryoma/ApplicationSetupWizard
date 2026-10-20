using System;
using System.IO;

namespace ApplicationSetupWizardLite.Paths
{
    public static class PathValidator
    {
        public static string ValidateSegment(
            string value,
            string propertyName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    $"{propertyName} が設定されていません。");
            }

            if (value == "." || value == "..")
            {
                throw new ArgumentException(
                    $"{propertyName} に不正な値が設定されています。");
            }

            if (Path.IsPathRooted(value) ||
                value.IndexOfAny(
                    Path.GetInvalidFileNameChars()) >= 0 ||
                value.Contains(Path.DirectorySeparatorChar) ||
                value.Contains(Path.AltDirectorySeparatorChar))
            {
                throw new ArgumentException(
                    $"{propertyName} に不正なパス要素が設定されています。");
            }

            return value;
        }
    }
}