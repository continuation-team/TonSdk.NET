using System.Collections;
using System.Text.RegularExpressions;

namespace TonSdk.Core.Boc;


public static partial class BitsPatterns {
     private const string BinaryString = @"^[01]+$";
     private const string HexString    = @"^([0-9a-fA-F]+|[0-9a-fA-F]*[1-9a-fA-F]+_?)$";
     private const string FiftBinary   = @"^b\{[01]+\}$";
     private const string FiftHex      = @"^x\{([0-9a-fA-F]+|[0-9a-fA-F]*[1-9a-fA-F]+_?)\}$";
     private const string Base64       = @"^(?:[A-Za-z0-9+\/]{4})*(?:[A-Za-z0-9+\/]{2}==|[A-Za-z0-9+\/]{3}=)?$";
     private const string Base64url    = @"^(?:[A-Za-z0-9-_]{4})*(?:[A-Za-z0-9-_]{2}==|[A-Za-z0-9-_]{3}=)?$";

     public static bool isBinaryString(this string s) {
          return BinaryStringRegex().IsMatch(s);
     }

     public static bool isHexString(this string s) {
          return HexStringRegex().IsMatch(s);
     }
     public static bool isFiftBinary(this string s) {
          return FiftBinaryRegex().IsMatch(s);
     }

     public static bool isFiftHex(this string s) {
          return FiftHexRegex().IsMatch(s);
     }

     public static bool isBase64(this string s) {
          return Base64Regex().IsMatch(s);
     }

     public static bool isBase64url(this string s) {
          return Base64urlRegex().IsMatch(s);
     }

#pragma warning disable CS8625
     [GeneratedRegex(BinaryString)]
     private static partial Regex BinaryStringRegex();

     [GeneratedRegex(HexString)]
     private static partial Regex HexStringRegex();

     [GeneratedRegex(FiftBinary)]
     private static partial Regex FiftBinaryRegex();

     [GeneratedRegex(FiftHex)]
     private static partial Regex FiftHexRegex();

     [GeneratedRegex(Base64)]
     private static partial Regex Base64Regex();
     #pragma warning restore CS8625

     [GeneratedRegex(Base64url)]
     private static partial Regex Base64urlRegex();
     #pragma warning restore CS8625
}

public class BitsEqualityComparer : IEqualityComparer<Bits> {
     public bool Equals(Bits x, Bits y) {
          if (x.Length != y.Length)
               return false;
          for (var i = 0; i < x.Length; i++) {
               if (x.Data[i] != y.Data[i])
                    return false;
          }
          return true;
     }

     public int GetHashCode(Bits obj) {
          var bits = obj.Hash();
          return bits.GetCopyTo(new int[8])[0];
     }
}

public static class BitArrayUtils {
     public static BitArray slice(this BitArray bits, int start, int end, bool inplace = false) {
          if (start < 0 || end < 0 || start > end || end > bits.Length) {
               throw new ArgumentException($"Invalid slice indexes: {start}, {end}");
          }
          var ret = inplace ? bits : (BitArray)bits.Clone();
          ret.RightShift(start);
          ret.Length = end - start;
          return ret;
     }
}
