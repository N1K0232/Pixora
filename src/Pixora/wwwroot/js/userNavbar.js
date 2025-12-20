document.addEventListener("alpine:init", () => {
    Alpine.data("userNavbar", () => ({
        isAuthenticated: false,
        userName: '',
        profilePhoto: null,
        isBusy: false,

        async init()
        {
            this.isBusy = true;
            try {
                await this.loadUser();
                if (this.isAuthenticated) {
                    await this.loadProfilePhoto();
                }
            } finally {
                this.isBusy = false;
            }
        },

        async loadUser()
        {
            const response = await fetch('/api/me', {
                credentials: 'include'
            });

            if (!response.ok) return;

            const content = await response.json();
            this.userName = content.userName;
            this.isAuthenticated = true;
        },

        async loadProfilePhoto()
        {
            const response = await fetch('/api/me/profilephoto', {
                credentials: 'include'
            });

            if (!response.ok) return;

            const blob = await response.blob();
            this.profilePhoto = URL.createObjectURL(blob);
        }
    }));
});