import Vue from 'vue';
import AppComponent from './App.vue';
import MessageBannerComponent from './components/MessageBanner.vue';
import AuthComponent from './components/Auth.vue';
import router from './router';

Vue.config.productionTip = false;

new Vue({
    router,
    components: {
        MessageBannerComponent,
        AuthComponent,
    },
  render: (h) => h(AppComponent),
}).$mount('#app');
