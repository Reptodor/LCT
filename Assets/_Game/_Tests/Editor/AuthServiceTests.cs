using System.IO;
using NUnit.Framework;

public class AuthServiceTests
{
    string _dir;

    [SetUp]
    public void SetUp()
    {
        _dir = Path.Combine(Path.GetTempPath(), "monetok-tests", System.Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }
    }

    [Test]
    public void RegisterAndLogin_KeepSeveralProfiles()
    {
        string path = Path.Combine(_dir, "accounts.json");
        var auth = new AuthService(path);

        Assert.IsTrue(auth.RegisterUser("A@Mail.com", "secret"));
        Assert.AreEqual("a@mail.com", auth.ActiveProfileId);
        Assert.IsFalse(auth.RegisterUser("a@mail.com", "other"));
        Assert.IsTrue(auth.RegisterUser("b@mail.com", "two"));

        auth.Logout();
        Assert.AreEqual(string.Empty, auth.ActiveProfileId);

        var again = new AuthService(path);
        Assert.IsFalse(again.LoginUser("a@mail.com", "wrong"));
        Assert.IsTrue(again.LoginUser("A@Mail.com", "secret"));
        Assert.AreEqual("a@mail.com", again.ActiveProfileId);
        Assert.IsTrue(again.LoginUser("b@mail.com", "two"));
        Assert.AreEqual("b@mail.com", again.ActiveProfileId);
    }

    [Test]
    public void ActiveProfile_SurvivesRestartUntilLogout()
    {
        string path = Path.Combine(_dir, "accounts.json");
        var auth = new AuthService(path);
        Assert.IsTrue(auth.RegisterUser("a@mail.com", "secret"));
        auth.Logout();
        Assert.IsTrue(auth.LoginUser("a@mail.com", "secret"));

        var restarted = new AuthService(path);
        Assert.AreEqual("a@mail.com", restarted.ActiveProfileId);
        Assert.IsNotNull(restarted.GetSavedCredentials());
        Assert.AreEqual("a@mail.com", restarted.GetSavedCredentials().Email);

        restarted.Logout();
        var signedOut = new AuthService(path);
        Assert.AreEqual(string.Empty, signedOut.ActiveProfileId);
        Assert.IsNull(signedOut.GetSavedCredentials());
    }

    [Test]
    public void LegacyAccount_IsImportedAndKeepsTheOldSave()
    {
        string path = Path.Combine(_dir, "accounts.json");
        var auth = new AuthService(path, "Old@Mail.com", "secret");

        Assert.IsTrue(auth.HasSavedCredentials());
        Assert.AreEqual(string.Empty, auth.ActiveProfileId);
        Assert.AreEqual("old@mail.com", auth.LegacySaveOwner);
        Assert.IsTrue(auth.LoginUser("old@mail.com", "secret"));
        Assert.IsTrue(auth.RegisterUser("new@mail.com", "two"));

        var again = new AuthService(path);
        Assert.AreEqual("old@mail.com", again.LegacySaveOwner);
        Assert.AreEqual("new@mail.com", again.ActiveProfileId);
    }
}
