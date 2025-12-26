function images(language)
{
    Alpine.data("images", () => ({
        file: null,
        image: {},
        stream: null,
        list: [],
        isBusy: false,
        errorMessage: null,

        getImage: async function (id)
        {
            this.isBusy = true;

            try
            {
                const response = await getImageAsync(id, language);
                const content = await response.json();

                this.errorMessage = GetErrorMessage(response.status, content);
                if (this.errorMessage == null)
                {
                    this.image = content;
                    this.stream = await getStreamAsync(id, language);
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

        getList: async function ()
        {
            this.isBusy = true;

            try
            {
                const response = await getListAsync(language);
                const content = await response.json();

                this.errorMessage = GetErrorMessage(response.status, content);
                if (this.errorMessage == null)
                {
                    this.list = content;
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

        deleteImage: async function (id)
        {
            this.isBusy = true;

            try
            {
                if (confirm("Are you sure you want to delete this image?"))
                {
                    const response = await deleteImageAsync(id, language);
                    const content = await response.json();

                    this.errorMessage = GetErrorMessage(response.status, content);
                    if (this.errorMessage == null)
                    {
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
        }
    }));
}

async function getImageAsync(id, language)
{
    const accessToken = window.localStorage.getItem('access_token');
    const response = await fetch(`/api/images/${id}`, {
        method: "GET",
        headers: {
            "Authorization": `Bearer ${accessToken}`,
            "Accept-Language": language
        }
    });

    return response;
}

async function getStreamAsync(id, language)
{
    const accessToken = window.localStorage.getItem('access_token');
    const response = await fetch(`/api/images/${id}/stream`, {
        method: "GET",
        headers: {
            "Authorization": `Bearer ${accessToken}`,
            "Accept-Language": language
        }
    });

    const blob = await response.blob();
    return URL.createObjectURL(blob);
}

async function getListAsync(language)
{
    const accessToken = window.localStorage.getItem('access_token');
    const response = await fetch('/api/images', {
        method: "GET",
        headers: {
            "Authorization": `Bearer ${accessToken}`,
            "Accept-Language": language
        }
    });

    return response;
}

async function deleteImageAsync(id, language)
{
    const accessToken = window.localStorage.getItem('access_token');
    const response = await fetch(`/api/images/${id}`, {
        method: "DELETE",
        headers: {
            "Authorization": `Bearer ${accessToken}`,
            "Accept-Language": language
        }
    });

    return response;
}