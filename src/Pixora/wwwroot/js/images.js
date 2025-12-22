function images(language)
{
    Alpine.data("images", () => ({
        list: [],
        isBusy: false,
        errorMessage: null,

        getList: async function () {
            this.isBusy = true;

            try {
                const response = await getListAsync(language);
                const content = await response.json();

                this.errorMessage = GetErrorMessage(response.status, content);
                if (this.errorMessage == null)
                {
                    for (let item of content)
                    {
                        this.list.push(await getImageStream(item.id, language));
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

async function getListAsync(language)
{
    const accessToken = window.localStorage.getItem('access_token');
    const response = await fetch('/api/images', {
        method: "GET",
        headers: {
            "Accept-Language": language,
            "Authorization": `Bearer ${accessToken}`
        }
    });

    return response;
}

async function getImageStream(id, language)
{
    const accessToken = window.localStorage.getItem('access_token');
    const response = await fetch(`/api/images/${id}/stream`, {
        method: "GET",
        headers: {
            "Accept-Language": language,
            "Authorization": `Bearer ${accessToken}`
        }
    });

    const blob = await response.blob();
    return URL.createObjectURL(blob);
}