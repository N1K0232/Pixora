document.addEventListener("alpine:init", () => {

    Alpine.store("user", {
        isAuthenticated: false,
        user: {},
        profilePhoto: null,
        isBusy: false,

        async load()
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

        async loadUser(accessToken)
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
            this.user = content;
            this.isAuthenticated = true;
        },

        async loadProfilePhoto(accessToken)
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
    });

    Alpine.data("userNavbar", () => ({
        init()
        {
            Alpine.store("user").load();
        },

        get isAuthenticated()
        {
            return Alpine.store("user").isAuthenticated;
        },

        get user()
        {
            return Alpine.store("user").user;
        },

        get profilePhoto()
        {
            return Alpine.store("user").profilePhoto;
        }
    }));
});