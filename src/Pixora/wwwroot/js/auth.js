function auth(language)
{
    Alpine.data("auth", () => ({
        firstName: '',
        lastName: '',
        email: '',
        userName: '',
        password: '',
        confirmPassword: '',
        qrCodeSrc: null,
        twoFactorCode: '',
        enableNotifications: false,
        isPersistent: false,
        isBusy: false,
        forgotPasswordMessage: null,
        errorMessage: null,

        confirmEmail: async function (secret, token)
        {
            this.isBusy = true;

            try
            {
                const response = await confirmEmailAsync(secret, token, language);
                if (response.status === 204)
                {
                    window.location.href = '/';
                }
                else
                {
                    const content = await response.json();
                    this.errorMessage = GetErrorMessage(response.status, text);
                }
            }
            catch (error)
            {
                this.errorMessage = error.message;
            }
            finally
            {
                this.isBusy = false;
            }
        },

        forgotPassword: async function ()
        {
            this.isBusy = true;

            try
            {
                const response = await forgotPasswordAsync(this.email, language);
                const content = await response.json();

                this.errorMessage = GetErrorMessage(response.status, content);
                if (this.errorMessage == null)
                {
                    this.forgotPasswordMessage = content.message;
                }
            }
            catch (error)
            {
                this.errorMessage = error.message;
            }
            finally
            {
                this.isBusy = false;
            }
        },

        getQrCode: async function ()
        {
            this.isBusy = true;

            try
            {
                const token = window.localStorage.getItem('twoFactorToken');
                const response = await getQrCodeAsync(token, language);

                if (response.status === 400)
                {
                    window.location.href = '/Account/ValidateTwoFactorCode';
                }
                else
                {
                    const blob = await response.blob();
                    this.qrCodeSrc = URL.createObjectURL(blob);
                }
            }
            catch (error)
            {
                this.errorMessage = error.message;
            }
            finally
            {
                this.isBusy = false;
            }
        },

        login: async function ()
        {
            this.isBusy = true;

            try
            {
                const response = await loginAsync(this.email, this.password, language);
                const content = await response.json();

                this.errorMessage = GetErrorMessage(response.status, content);
                if (this.errorMessage == null)
                {
                    if (content.twoFactorToken != null)
                    {
                        window.localStorage.setItem('twoFactorToken', content.twoFactorToken);
                        window.location.href = '/Account/QrCodeImage';
                    }
                    else
                    {
                        window.localStorage.setItem('access_token', content.accessToken);
                        window.localStorage.setItem('refresh_token', content.refreshToken);
                        window.location.href = '/';
                    }
                }
            }
            catch (error)
            {
                this.errorMessage = error.message;
            }
            finally
            {
                this.isBusy = false;
            }
        },

        logout: async function ()
        {
            this.isBusy = true;

            try
            {
                const response = await logoutAsync(language);
                if (response.status === 204)
                {
                    window.localStorage.removeItem('access_token');
                    window.localStorage.removeItem('refresh_token');
                    window.location.href = '/';
                }
                else
                {
                    const content = await response.json();
                    this.errorMessage = GetErrorMessage(response.status, content);
                }
            }
            catch (error)
            {
                this.errorMessage = error.message;
            }
            finally
            {
                this.isBusy = false;
            }
        },

        next: function ()
        {
            window.location.href = '/Account/ValidateTwoFactorCode';
        },

        refresh: async function ()
        {
            this.isBusy = true;

            try
            {
                const accessToken = window.localStorage.getItem('access_token');
                const refreshToken = window.localStorage.getItem('refresh_token');

                const response = await refreshTokenAsync(accessToken, refreshToken, language);
                const content = await response.json();

                this.errorMessage = GetErrorMessage(response.status, content);
                if (this.errorMessage == null)
                {
                    window.location.href = '/';
                }
            }
            catch (error)
            {
                this.errorMessage = error.message;
            }
            finally
            {
                this.isBusy = false;
            }
        },

        register: async function ()
        {
            this.isBusy = true;

            try
            {
                const response = await registerAsync(this.firstName, this.lastName, this.email, this.userName, this.password, this.confirmPassword, this.enableNotifications, language);
                const content = await response.json();

                this.errorMessage = GetErrorMessage(response.status, content);
            }
            catch (error)
            {
                this.errorMessage = error.message;
            }
            finally
            {
                this.isBusy = false;
            }
        },

        resetPassword: async function (secret, token)
        {
            this.isBusy = true;

            try
            {
                const response = await resetPasswordAsync(secret, token, this.password, this.confirmPassword, language);
                if (response.status === 204)
                {
                    window.location.href = '/';
                }
                else
                {
                    const content = await response.json();
                    this.errorMessage = GetErrorMessage(response.status, content);
                }
            }
            catch (error)
            {
                this.errorMessage = error.message;
            }
            finally
            {
                this.isBusy = false;
            }
        },

        validateTwoFactor: async function ()
        {
            this.isBusy = true;

            try
            {
                const token = window.localStorage.getItem('twoFactorToken');
                const response = await validateTwoFactorAsync(token, this.twoFactorCode, language);

                const content = await response.json();
                this.errorMessage = GetErrorMessage(response.status, content);

                if (this.errorMessage == null)
                {
                    window.localStorage.setItem('access_token', content.accessToken);
                    window.localStorage.setItem('refresh_token', content.refreshToken);

                    window.localStorage.removeItem('twoFactorToken');
                    window.location.href = '/';
                }
            }
            catch (error)
            {
                this.errorMessage = error.message;
            }
            finally
            {
                this.isBusy = false;
            }
        }
    }));
}

async function confirmEmailAsync(secret, token, language)
{
    const response = await fetch(`/api/auth/confirm?secret=${secret}&token=${token}`, {
        method: "GET",
        headers: {
            "Accept-Language": language
        }
    });

    return response;
}

async function forgotPasswordAsync(email, language)
{
    const request = { email: email };

    const response = await fetch('/api/auth/forgotpassword', {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": language
        },
        body: JSON.stringify(request)
    });

    return response;
}

async function getQrCodeAsync(token, language)
{
    const response = await fetch(`/api/auth/qrcode?token=${token}`, {
        method: "GET",
        headers: {
            "Accept-Language": language
        }
    });

    return response;
}

async function loginAsync(email, password, language)
{
    const request = {
        email: email,
        password: password
    };

    const response = await fetch('/api/auth/login', {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": language
        },
        body: JSON.stringify(request)
    });

    return response;
}

async function logoutAsync(language)
{
    const accessToken = window.localStorage.getItem('access_token');
    const response = await fetch('/api/auth/logout', {
        method: "POST",
        headers: {
            "Authorization": `Bearer ${accessToken}`,
            "Accept-Language": language
        }
    });

    return response;
}

async function refreshTokenAsync(accessToken, refreshToken, language)
{
    const request = {
        accessToken: accessToken,
        refreshToken: refreshToken
    };

    const response = await fetch('/api/auth/refresh', {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": language
        },
        body: JSON.stringify(request)
    });

    return response;
}

async function registerAsync(firstName, lastName, email, userName, password, confirmPassword, enableNotifications, language)
{
    const request = {
        firstName: firstName,
        lastName: lastName,
        email: email,
        userName: userName,
        password: password,
        confirmPassword: confirmPassword,
        enableNotifications: enableNotifications
    };

    const response = await fetch('/api/auth/register', {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": language
        },
        body: JSON.stringify(request)
    });

    return response;
}

async function resetPasswordAsync(secret, token, newPassword, confirmPassword, language)
{
    const request = {
        secret: secret,
        token: token,
        newPassword: newPassword,
        confirmPassword: confirmPassword
    };

    const response = await fetch('/api/auth/resetpassword', {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": language
        },
        body: JSON.stringify(request)
    });

    return response;
}

async function validateTwoFactorAsync(token, code, language)
{
    const request = {
        token: token,
        code: code
    };

    const response = await fetch('/api/auth/validate2fa', {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": language
        },
        body: JSON.stringify(request)
    });

    return response;
}