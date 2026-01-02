function chat(language)
{
    Alpine.data("chat", () => ({
        socket: null,
        messages: [],
        text: '',
        connected: false,
        conversationId: uuid(),

        init: function ()
        {
            const token = window.localStorage.getItem('access_token');
            if (!token)
            {
                console.warn('jwt missing');
                return;
            }

            const url = `wss://${location.host}/ws/chat?access_token=${token}`;
            this.socket = new WebSocket(url);

            this.socket.onopen = () =>
            {
                this.connected = true;
                this.send({ type: 'message:list', conversationId: this.conversationId, take: 50 });
            };

            this.socket.onmessage = (e) =>
            {
                const msg = JSON.parse(e.data);
                this.handleMessage(msg);
            };

            this.socket.onclose = () =>
            {
                this.connected = false;
            };
        },

        handleMessage: function (msg)
        {
            switch (msg.type)
            {
                case "message":
                    this.messages.push(msg);
                    break;

                case "message:list":
                    this.messages = msg.items ?? [];
                    break;
            }
        },

        send: function (payload)
        {
            if (!this.connected)
            {
                return;
            }

            this.socket.send(JSON.stringify(payload));
        },

        sendMessage: function ()
        {
            if (!this.text.trim())
            {
                return;
            }

            this.send({ type: "message:send", conversationId: this.conversationId, content: this.text });
            this.text = '';
        }
    }));
}