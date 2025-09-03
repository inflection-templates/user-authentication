using System;
using System.Security.Cryptography;
using shala.api.domain.types;
using shala.api.services;

namespace shala.api.common;

public static class Helper
{
    public static string GenerateRandomString(int len = 10)
    {
        var str = string.Empty;
        var random = new Random();
        for (int i = 0; i < len; i++)
        {
            var randValue = random.Next(0, 26);
            var letter = Convert.ToChar(randValue + 65);
            str = str + letter;
        }
        return str;
    }

    public static string GenerateUserName(string? firstName, string? lastName)
    {
        var username = string.Empty;

        if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
        {
            username = GenerateRandomString(10);
        }

        if (!string.IsNullOrEmpty(firstName) && firstName.Length > 0)
        {
            username += firstName.Substring(0, 3);
        }
        if (!string.IsNullOrEmpty(lastName) && lastName.Length > 0)
        {
            username += lastName.Substring(0, 3);
        }

        username = username.ToLower();
        username = username.Trim();
        username = username.Replace("@", "");
        username = username.Replace("-", "");
        username = username.Replace("_", "");
        username = username.Replace("_", "");
        username = username.Replace(".", "");

        var random = new Random();

        if (username.Length > 5)
        {
            username = username.Substring(0, 6);
        }
        username += random.Next(0, 10000000).ToString();
        username = username.Substring(0, 8);

        return username;
    }

    public static async Task<string?> GetUniqueUsername(IUserService service, string? firstName, string? lastName)
    {
        var username = GenerateUserName(firstName, lastName);
        var user = await service.GetByUsernameAsync(username);
        if (user != null)
        {
            return GenerateUserName(null, null);
        }
        return username;
    }

    public static string GenerateTenantCode(string? name)
    {
        var code = string.Empty;
        if (!string.IsNullOrEmpty(name))
        {
            code = name.Substring(0, 3);
        }
        code = code.ToLower();
        code = code.Trim();
        code = code.Replace("@", "");
        code = code.Replace("-", "");
        code = code.Replace("_", "");
        code = code.Replace("_", "");
        code = code.Replace(".", "");

        var random = new Random();

        if (code.Length > 5)
        {
            code = code.Substring(0, 6);
        }
        code += random.Next(0, 10000000).ToString();
        code = code.Substring(0, 8);

        return code;
    }

    public static async Task<string?> GetUniqueTenantCode(ITenantService service, string? name)
    {
        var code = GenerateTenantCode(name);
        var tenant = await service.GetByCodeAsync(code);
        if (tenant != null)
        {
            return GenerateTenantCode(null);
        }
        return code;
    }

    public static string GenerateKey(int keyLength)
    {
        // Generate a cryptographically secure random key
        using (var rng = RandomNumberGenerator.Create())
        {
            byte[] keyBytes = new byte[keyLength];
            rng.GetBytes(keyBytes);

            // Convert the byte array to a Base64 string
            return Convert.ToBase64String(keyBytes)
                .Replace("+", "")    // Remove '+' to avoid URL encoding issues
                .Replace("/", "")    // Remove '/' to avoid URL encoding issues
                .Replace("=", "");   // Remove '=' padding for a clean string
        }
    }

    public static string GetRoleName(DefaultRoles role)
    {
        var rolesWithDescriptions = EnumHelper.GetEnumWithNameAndDescriptions<DefaultRoles>();
        var x = rolesWithDescriptions.FirstOrDefault(r => r.EnumValue == role);
        return x.Name ?? string.Empty;
    }

    public static string GenerateTotpSecret(int length = 32)
    {
        byte[] secretKey = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(secretKey);
        }

        // Convert to Base32 or Base64 (Base32 is more common for TOTP)
        return Base32Encode(secretKey);
    }

    private static string Base32Encode(byte[] data)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        int bits = 0;
        int value = 0;
        string result = "";

        foreach (byte b in data)
        {
            bits += 8;
            value = (value << 8) | b;

            while (bits >= 5)
            {
                result += alphabet[(value >> (bits - 5)) & 0x1F];
                bits -= 5;
            }
        }

        if (bits > 0)
        {
            result += alphabet[(value << (5 - bits)) & 0x1F];
        }

        return result;
    }

}
