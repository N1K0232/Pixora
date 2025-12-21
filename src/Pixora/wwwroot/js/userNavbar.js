document.addEventListener("alpine:init", () => {
    Alpine.data("userNavbar", () => ({
        isAuthenticated: false,
        userName: '',
        profilePhoto: null,
        isBusy: false,

        init: async function ()
        {
            this.isBusy = true;
            try
            {
                const accessToken = window.localStorage.getItem('access_token');
                await this.loadUser(accessToken);

                if (this.isAuthenticated)
                {
                    await this.loadProfilePhoto(accessToken);
                }
            }
            catch (error)
            {
                alert(error.message);
            }
            finally
            {
                this.isBusy = false;
            }
        },

        loadUser: async function (accessToken)
        {
            const response = await fetch('/api/me', {
                method: "GET",
                headers: {
                    "Authorization": `Bearer ${accessToken}`,
                    "Accept-Language": "en-US"
                },
                credentials: 'include'
            });

            if (!response.ok)
            {
                return;
            }

            const content = await response.json();
            this.userName = content.userName;
            this.isAuthenticated = true;
        },

        loadProfilePhoto: async function (accessToken)
        {
            const response = await fetch('/api/me/profilephoto', {
                method: "GET",
                headers: {
                    "Authorization": `Bearer ${accessToken}`,
                    "Accept-Language": "en-US"
                },
                credentials: 'include'
            });

            if (!response.ok)
            {
                return;
            }

            const blob = await response.blob();
            this.profilePhoto = URL.createObjectURL(blob);
        }
    }));
});