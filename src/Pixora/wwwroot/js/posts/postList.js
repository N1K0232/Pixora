function postList(language)
{
    Alpine.data("postList", () => ({
        posts: [],
        isBusy: false,
        errorMessage: null,

        init: async function ()
        {
            this.isBusy = true;

            try
            {
                const response = await getPostsAsync(language);
                const content = await response.json();

                this.errorMessage = GetErrorMessage(response.status, content);
                if (this.errorMessage == null)
                {
                    this.posts = content;
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

async function getPostsAsync(language)
{
    const accessToken = window.localStorage.getItem('accessToken');
    const response = await fetch('/api/posts', {
        method: "GET",
        headers: {
            "Authorization": `Bearer ${accessToken}`,
            "Accept-Language": language
        }
    });

    return response;
}