using System.Collections;
using System.Text.RegularExpressions;

namespace TonSdk.Core.Boc;


public static partial class BitsPatterns {
     private const string BinaryString = @"^[01]+$";
     private const string HexString = @"^([0-9a-fA-F]+|[0-9a-fA-F]*[1-9a-fA-F]+_?)$";
     private const string FiftBinary = @"^b\{[01]+\}$";
     private const string FiftHex = @"^x\{([0-9a-fA-F]+|[0-9a-fA-F]*[1-9a-fA-F]+_?)\}$";
     private const string Base64 = @"^(?:[A-Za-z0-9+\/]{4})*(?:[A-Za-z0-9+\/]{2}==|[A-Za-z0-9+\/]{3}=)?$";

     private static void throwUnlessMatch(this bool x, string description) {
          if (!x) {
               throw new ArgumentException(description);
          }
     }

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

     public static void checkIsBinaryString(this string s) {
          s.isBinaryString().throwUnlessMatch($"{s} is not BitString, BitString is 1000101101010");
     }
     public static void checkIsHexString(this string s) {
          s.isHexString().throwUnlessMatch($"{s} is not HexString, HexString is 1F419ADB7");
     }
     public static void checkIsFiftBinary(this string s) {
          s.isFiftBinary().throwUnlessMatch($"{s} is not FiftBits, FiftBits is b{{1001010}}");
     }
     public static void checkIsFiftHex(this string s) {
          s.isFiftHex().throwUnlessMatch($"{s} is not FiftHex, FiftHex is x{{1AB95FF}}");
     }

     public static void checkIsBase64(this string s) {
          s.isBase64().throwUnlessMatch($"{s} is not Base64, Base64 is te6ccuEBAQEAKwBWAFEAAAAyKam=");
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
          var bits = obj.hash().Parse().readBits(32);
          return bits.getCopyTo(new int[1])[0];
     }
}

public static class BitArrayUtils {
     public static BitArray slice(this BitArray bits, int start, int end, bool inplace = false) {
          var ret = inplace ? bits : (BitArray)bits.Clone();
          ret.RightShift(start);
          ret.Length = end - start;
          return ret;
     }
}
