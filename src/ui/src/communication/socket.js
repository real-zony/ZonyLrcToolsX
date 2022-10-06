import Vue from 'vue'
import eventBus from "@/communication/eventbus";

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
                let event = JSON.parse(msg.data);
                eventBus.$emit(event.action, event);
            };
            socket.onerror = (err) => {
                eventBus.$emit('error', err);
            };
            socket.onclose = () => {
                emitter.connect()
            };
        }
    }
})

emitter.connect();
export default emitter;