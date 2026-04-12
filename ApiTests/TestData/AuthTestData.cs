using System;
using System.Collections.Generic;
using System.Text;

namespace ApiTests.TestData;

public static class AuthTestData
{
    public static TheoryData<string, string> ValidUsers => new()
    {
        {"user1", "Password1!" },
        {"user2", "Secure2@A"},
        {"user3", "Valid3#Pass"},
        {"user4", "Strong4$Pwd"},
        {"user5", "Alpha5%Beta"},
        {"user6", "Gamma6^Delta"},
        {"user7", "Theta7&Omega"},
        {"user8", "Lambda8*Phi"},
        {"user9", "Sigma9!Tau"},
        {"user10", "Zeta10@Eta"},
        {"user_11", "Kappa11#Mu"},
        {"user_12", "Nu12$XiAa"},
        {"user_13", "Omicron13%Pi"},
        {"user_14", "Rho14^Sigma"},
        {"user_15", "Tau15&Upsilon"},
        {"user_16", "Phi16*ChiAa"},
        {"user_17", "Psi17!Omega"},
        {"user_18", "Alpha18@Beta"},
        {"user_19", "Gamma19#Delta"},
        {"user_20", "Theta20$Lambda"},
        {"User21", "Sigma21%Tau"},
        {"User22", "Zeta22^EtaAa"},
        {"User23", "Kappa23&Mu"},
        {"User24", "Nu24*XiAa"},
        {"User25", "Omicron25!Pi"},
        {"User26", "Rho26@Sigma"},
        {"User27", "Tau27#Upsilon"},
        {"User28", "Phi28$ChiAa"},
        {"User29", "Psi29%Omega"},
        {"User30", "Alpha30^Beta"},
        {"testUser31", "Gamma31&Delta"},
        {"testUser32", "Theta32*Lambda"},
        {"testUser33", "Sigma33!Tau"},
        {"testUser34", "Zeta34@EtaAa"},
        {"testUser35", "Kappa35#Mu"},
        {"testUser36", "Nu36$XiAa"},
        {"testUser37", "Omicron37%Pi"},
        {"testUser38", "Rho38^Sigma"},
        {"testUser39", "Tau39&Upsilon"},
        {"testUser40", "Phi40*ChiAa"},
        {"alpha_41", "Psi41!Omega"},
        {"beta_42", "Alpha42@Beta"},
        {"gamma_43", "Gamma43#Delta"},
        {"delta_44", "Theta44$Lambda"},
        {"epsilon_45", "Sigma45%Tau"},
        {"zeta_46", "Zeta46^EtaAa"},
        {"eta_47", "Kappa47&Mu"},
        {"theta_48", "Nu48*XiAa"},
        {"iota_49", "Omicron49!Pi"},
        {"kappa_50", "Rho50@Sigma"},
        {"töstuser", "Password1!"},
        {"mañana123", "Secure2@A"},
        {"über_user", "Valid3#Pass"},
        {"façade99", "Strong4$Pwd"},
        {"niño_123", "Alpha5%Beta"},
        {"用户123", "Gamma6^Delta"},
        {"测试用户", "Theta7&Omega"},
        {"数据_user", "Lambda8*Phi"},
        {"пользователь1", "Sigma9!Tau"},
        {"тест_юзер", "Zeta10@Eta"},
        {"ユーザー123", "Kappa11#Mu"},
        {"テストユーザー", "Nu12$XiAa"},
        {"사용자123", "Omicron13%Pi"},
        {"테스트유저", "Rho14^Sigma"},
        {"utilisateuré", "Tau15&Upsilon"},
        {"crème_user", "Phi16*ChiAa"},
        {"δοκιμή123", "Psi17!Omega"},
        {"χρήστης_1", "Alpha18@Beta"},
        {"مستخدم123", "Gamma19#Delta"},
        {"اختبار_مستخدم", "Theta20$Lambda"},
        { "EmojiUser2", "😀Password1!"}
    };

    // Username, Password, Reason for invalidity
    public static TheoryData<string, string, string> InValidUsers => new()
    {
        { "ab", "Password1!", "Username too short (min 3 characters)" },

        { "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "Password1!", "Username too long (max 50 characters)" },

        { "_username", "Password1!", "Username cannot start with underscore" },
        { "username_", "Password1!", "Username cannot end with underscore" },

        { "user!name", "Password1!", "Username contains invalid character '!'" },
        { "user name", "Password1!", "Username cannot contain spaces" },

        { "validuser", "P1!a", "Password too short (min 8 characters)" },

        { "validuser", "password1!", "Password missing uppercase letter" },
        { "validuser", "PASSWORD1!", "Password missing lowercase letter" },

        { "validuser", "Password!", "Password missing number" },

        { "validuser", "Password1", "Password missing special character" },

        { "validuser", "Password 1!", "Password cannot contain whitespace" },

        { "validuser", "Password", "Password missing number and special character" },

        { "validuser", "12345678!", "Password missing letters" },

        { "validuser", "password123", "Password missing uppercase and special character" },

        { "validuser", "PASSWORD123", "Password missing lowercase and special character" },

        { "validuser", "short", "Password missing multiple requirements (length, uppercase, number, special char)" },

        { "", "Password1!", "Username cannot be empty" },
        { "validuser", "", "Password cannot be empty" },

        { "   ", "Password1!", "Username cannot be whitespace" },
        { "validuser", "   ", "Password cannot be whitespace" },

        { "_bad", "short", "Username invalid (leading underscore) and password too weak" },

        { "gooduser", "abc", "Password too weak (missing length, uppercase, number, special char)" },

        { "bad user", "Password1!", "Username cannot contain spaces" },

        { "___", "Password1!", "Username cannot start or end with underscore" },

        { "user-name", "Password1!", "Username contains invalid character '-'" },

        { "user.name", "Password1!", "Username contains invalid character '.'" },

        { "EmojiUser1😀", "Password1!", "Username contains an emoji"}
    };
}
