import Vue from 'vue'

// const wsUrl = process.env.VUE_APP_WS_URL;
const wsUrl = "ws://127.0.0.1:50001"

let socket = new WebSocket(wsUrl);
const emitter = new Vue({
    methods: {
        send(message) {
            if (socket.readyState === 1) {
                socket.send(JSON.stringify(message));
            }
        }, connect() {
            socket = new WebSocket(wsUrl);
            socket.onmessage = (msg) => {
                this.$emit('message', msg.data);
            };
            socket.onerror = (err) => {
                this.$emit('error', err);
            };
            socket.onclose = () => {
                emitter.connect()
            };
        }
    }
})

emitter.connect();
export default emitter;