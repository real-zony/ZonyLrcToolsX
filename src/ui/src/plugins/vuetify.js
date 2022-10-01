import Vue from 'vue';
import Vuetify from 'vuetify/lib/framework';

Vue.use(Vuetify);

export default new Vuetify({
    theme: {
        dark: false,
        themes: {
            light: {
                primary: '#03a9f4',
                secondary: '#8bc34a',
                accent: '#cddc39',
                error: '#f44336',
                warning: '#ffc107',
                info: '#607d8b',
                success: '#4caf50'
            }
        },
    }
});