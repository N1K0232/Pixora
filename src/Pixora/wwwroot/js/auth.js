function auth(language)
{
    Alpine.data("auth", () => ({
        firstName: '',
        lastName: '',
        email: '',
        userName: '',
        password: '',
        enableNotifications: false,
        isPersistent: false,
        isBusy: false,
        errorMessage: '',

        login: async function () {
            this.isBusy = true;

            try {
                const response = await loginAsync(this.email, this.password, language);
                const content = await response.json();

                this.errorMessage = GetErrorMessage(response.status, content);
                if (this.errorMessage == null) {
                    setAuthCookie('jwtBearer', content.accessToken, content.refreshToken, this.isPersistent);
                }
            }
            catch (error) {
                this.errorMessage = error.message;
            }
            finally {
                this.isBusy = false;
            }
        },

        refresh: async function () {
            this.isBusy = true;

            try {
                const accessToken = window.localStorage.getItem('access_token');
                const refreshToken = window.localStorage.getItem('refresh_token');

                const response = await refreshTokenAsync(accessToken, refreshToken, language);
                const content = await response.json();

                this.errorMessage = GetErrorMessage(response.status, content);
                if (this.errorMessage == null) {
                    setAuthCookie('jwtBearer', content.accessToken, content.refreshToken, false);
                }
            }
            catch (error) {
                this.errorMessage = error.message;
            }
            finally {
                this.isBusy = false;
            }
        },

        register: async function () {
            this.isBusy = true;

            try {
                const response = await registerAsync(this.firstName, this.lastName, this.email, this.userName, this.password, this.enableNotifications, language);
                const content = await response.json();

                this.errorMessage = GetErrorMessage(response.status, content);
            }
            catch (error) {
                this.errorMessage = error.message;
            }
            finally {
                this.isBusy = false;
            }
        }
    }));
}

async function loginAsync(email, password, language) {
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

async function refreshTokenAsync(accessToken, refreshToken, language) {
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

async function registerAsync(firstName, lastName, email, userName, password, enableNotifications, language) {
    const request = {
        firstName: firstName,
        lastName: lastName,
        email: email,
        userName: userName,
        password: password,
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