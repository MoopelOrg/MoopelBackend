namespace ApiTests.TestData;

public static class AuthTestData
{
    public const string ValidUserName = "validUser1";
    public const string ValidPassword = "pAssword1#!";

    public const string ValidUserName2 = "validUser2";
    public const string ValidPassword2 = "pAssword1#!";


    public static IEnumerable<(string Username, string Password)> ValidUserValues()
    {
        yield return (ValidUserName, ValidPassword);
        yield return (ValidUserName2, ValidPassword2);
        yield return ("user1", "Password1!");
        yield return ("user2", "Secure2@A");
        yield return ("user3", "Valid3#Pass");
        yield return ("user4", "Strong4$Pwd");
        yield return ("user5", "Alpha5%Beta");
        yield return ("user6", "Gamma6^Delta");
        yield return ("user7", "Theta7&Omega");
        yield return ("user8", "Lambda8*Phi");
        yield return ("user9", "Sigma9!Tau");
        yield return ("user10", "Zeta10@Eta");
        yield return ("user_11", "Kappa11#Mu");
        yield return ("user_12", "Nu12$XiAa");
        yield return ("user_13", "Omicron13%Pi");
        yield return ("user_14", "Rho14^Sigma");
        yield return ("user_15", "Tau15&Upsilon");
        yield return ("user_16", "Phi16*ChiAa");
        yield return ("user_17", "Psi17!Omega");
        yield return ("user_18", "Alpha18@Beta");
        yield return ("user_19", "Gamma19#Delta");
        yield return ("user_20", "Theta20$Lambda");
        yield return ("User21", "Sigma21%Tau");
        yield return ("User22", "Zeta22^EtaAa");
        yield return ("User23", "Kappa23&Mu");
        yield return ("User24", "Nu24*XiAa");
        yield return ("User25", "Omicron25!Pi");
        yield return ("User26", "Rho26@Sigma");
        yield return ("User27", "Tau27#Upsilon");
        yield return ("User28", "Phi28$ChiAa");
        yield return ("User29", "Psi29%Omega");
        yield return ("User30", "Alpha30^Beta");
        yield return ("testUser31", "Gamma31&Delta");
        yield return ("testUser32", "Theta32*Lambda");
        yield return ("testUser33", "Sigma33!Tau");
        yield return ("testUser34", "Zeta34@EtaAa");
        yield return ("testUser35", "Kappa35#Mu");
        yield return ("testUser36", "Nu36$XiAa");
        yield return ("testUser37", "Omicron37%Pi");
        yield return ("testUser38", "Rho38^Sigma");
        yield return ("testUser39", "Tau39&Upsilon");
        yield return ("testUser40", "Phi40*ChiAa");
        yield return ("alpha_41", "Psi41!Omega");
        yield return ("beta_42", "Alpha42@Beta");
        yield return ("gamma_43", "Gamma43#Delta");
        yield return ("delta_44", "Theta44$Lambda");
        yield return ("epsilon_45", "Sigma45%Tau");
        yield return ("zeta_46", "Zeta46^EtaAa");
        yield return ("eta_47", "Kappa47&Mu");
        yield return ("theta_48", "Nu48*XiAa");
        yield return ("iota_49", "Omicron49!Pi");
        yield return ("kappa_50", "Rho50@Sigma");
        yield return ("töstuser", "Password1!");
        yield return ("mañana123", "Secure2@A");
        yield return ("über_user", "Valid3#Pass");
        yield return ("façade99", "Strong4$Pwd");
        yield return ("niño_123", "Alpha5%Beta");
        yield return ("用户123", "Gamma6^Delta");
        yield return ("测试用户", "Theta7&Omega");
        yield return ("数据_user", "Lambda8*Phi");
        yield return ("пользователь1", "Sigma9!Tau");
        yield return ("тест_юзер", "Zeta10@Eta");
        yield return ("ユーザー123", "Kappa11#Mu");
        yield return ("テストユーザー", "Nu12$XiAa");
        yield return ("사용자123", "Omicron13%Pi");
        yield return ("테스트유저", "Rho14^Sigma");
        yield return ("utilisateuré", "Tau15&Upsilon");
        yield return ("crème_user", "Phi16*ChiAa");
        yield return ("δοκιμή123", "Psi17!Omega");
        yield return ("χρήστης_1", "Alpha18@Beta");
        yield return ("مستخدم123", "Gamma19#Delta");
        yield return ("اختبار_مستخدم", "Theta20$Lambda");
        yield return ("EmojiUser2", "😀Password1!");
    }

    public static TheoryData<string, string> ValidUsers
    {
        get
        {
            TheoryData<string, string> data = [];

            foreach ((string username, string password) in ValidUserValues())
            {
                data.Add(username, password);
            }

            return data;
        }
    }


    public static IEnumerable<(string Username, string Password, string Reason)> InvalidUserValues()
    {
        yield return ("ab", "Password1!", "Username too short (min 3 characters)");

        yield return ("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "Password1!", "Username too long (max 50 characters)");

        yield return ("_username", "Password1!", "Username cannot start with underscore");
        yield return ("username_", "Password1!", "Username cannot end with underscore");

        yield return ("user!name", "Password1!", "Username contains invalid character '!'");
        yield return ("user name", "Password1!", "Username cannot contain spaces");

        yield return ("validuser", "P1!a", "Password too short (min 8 characters)");

        yield return ("validuser", "password1!", "Password missing uppercase letter");
        yield return ("validuser", "PASSWORD1!", "Password missing lowercase letter");

        yield return ("validuser", "Password!", "Password missing number");

        yield return ("validuser", "Password1", "Password missing special character");

        yield return ("validuser", "Password 1!", "Password cannot contain whitespace");

        yield return ("validuser", "Password", "Password missing number and special character");

        yield return ("validuser", "12345678!", "Password missing letters");

        yield return ("validuser", "password123", "Password missing uppercase and special character");

        yield return ("validuser", "PASSWORD123", "Password missing lowercase and special character");

        yield return ("validuser", "short", "Password missing multiple requirements (length, uppercase, number, special char)");

        yield return ("", "Password1!", "Username cannot be empty");
        yield return ("validuser", "", "Password cannot be empty");

        yield return ("   ", "Password1!", "Username cannot be whitespace");
        yield return ("validuser", "   ", "Password cannot be whitespace");

        yield return ("_bad", "short", "Username invalid (leading underscore) and password too weak");

        yield return ("gooduser", "abc", "Password too weak (missing length, uppercase, number, special char)");

        yield return ("bad user", "Password1!", "Username cannot contain spaces");

        yield return ("___", "Password1!", "Username cannot start or end with underscore");

        yield return ("user-name", "Password1!", "Username contains invalid character '-'");

        yield return ("user.name", "Password1!", "Username contains invalid character '.'");

        yield return ("EmojiUser1😀", "Password1!", "Username contains an emoji");
    }

    // Username, Password, Reason for invalidity
    public static TheoryData<string, string, string> InValidUsers
    {
        get
        {
            TheoryData<string, string, string> data = [];

            foreach ((string username, string password, string reason) in InvalidUserValues())
            {
                data.Add(username, password, reason);
            }

            return data;
        }
    }
}
