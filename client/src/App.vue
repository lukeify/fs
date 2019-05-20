<template>
    <div>
        <message-banner></message-banner>

        <h1>fs.lukeify.com</h1>

        <auth v-if="!authenticated" v-on:authenticate="authenticated = true"></auth>

        <transition name="authed">
            <home v-if="authenticated"></home>
        </transition>

        <footer class="footer">
            <ul class="footer-list">
                <li class="postface footer-list-item">
                    <span>Written by <a href="https://twitter.com/lukeifynz">@lukeify</a> (<a href="https://github.com/lukeify/fs">repo</a>).</span>
                </li>
                <li v-if="env === 'development'" class="footer-list-item">
                    <a v-on:click="wipe">Wipe</a>
                </li>
                <li v-if="authenticated" class="footer-list-item">
                    <a class="deauth" v-on:click="deauth">De-auth</a>
                </li>
            </ul>
        </footer>
    </div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import EventBus from './EventBus';

import AuthComponent from '@/components/Auth.vue';
import HomeComponent from '@/components/Home.vue';
import MessageBannerComponent from '@/components/MessageBanner.vue';
import EnvironmentService from '@/services/EnvironmentService';

@Component({
    name: 'App',
    components: {
        'auth': AuthComponent, 'home': HomeComponent, 'message-banner': MessageBannerComponent,
    },
})
export default class AppComponent extends Vue {
    public env: string = EnvironmentService.getEnv;
    public authenticated: boolean = false;

    /**
     * When the component is created, begin monitoring for copy requests & message-banner events
     * made across the application.
     */
    public created(): void {
        EventBus.$on('copy', this.onCopyEvent.bind(this));
        EventBus.$on('message-banner', (messageData: string) => {
            // non-empty block
        });
    }

    /**
     * When a copy event occurs, select the text, and attempt to copy the value. If this is successful,
     * the URL will be copied onclick.
     */
    private onCopyEvent(element: HTMLInputElement): void {
        if (element != null && typeof element.select === 'function') {
            // select text
            element.select();

            try {
                document.execCommand('copy');
                element.blur();
                // Use Web Notifications API
            } catch (e) {
                console.log(e);
            }
        }
    }

    /**
     * Deauth wipes the local storage key, and sets the authentication flag to false.
     */
    private deauth(): void {
        localStorage.removeItem('appKey');
        this.authenticated = false;
    }

    /**
     * Removes all files from the application, deleting them in the database and also on disk. Only works in development.
     * Once complete, emits a 'wipeComplete' event which other components will listen for.
     */
    private wipe(): void {
        if (window.confirm(`Are you sure you want to wipe fs.lukeify.com?`)) {
            fetch('/api/dev/wipe', { method: 'GET' }).then((res) => {
                EventBus.$emit('wipeComplete');
            });
        }
    }
}
</script>

<style lang="scss">
    @import "styles/design.scss";

    html {
        margin: 0 100px;
    }

    html, body {
        background-color: $white;
        color: $grey;

        @media (prefers-color-scheme: dark) {
            background-color:$midnight;
            color:$white;
        }
    }

    body {
        text-align: center;
        margin: 0;
        padding: 0;
        margin-bottom: 10em;
    }

    h1, h2, h3, h4, h5, h6, p, a, ul, li, span {
        font-family: $inconsolata;
    }

    h1 {
        font-size: 3em;
        text-align: center;
        margin: 1em;
        padding: 0;
    }

    ul {
        list-style-type: none;
        margin: 0;
        padding: 0;
    }

    a {
        color: $emerald;
        cursor: pointer;
        transition:transform 100ms ease;
        display:inline-block;
    }

    a:hover, a:active {
        color: $malachite;
    }

    a:active {
        transform:scale(0.9);
    }

    .authed-enter-active, .authed-leave-active {
        transition: opacity 0.5s ease-in-out;
        opacity: 1;
    }

    .authed-enter, .authed-leave-to {
        opacity: 0;
    }

    .footer {
        position: fixed;
        padding: 1.3em 100px 1.5em 100px;
        bottom: 0;
        left: 0;
        right:0;
        background-color:rgba($white, 0.6);
        backdrop-filter: blur(30px) saturate(200%);
        text-align: left;
        font-size: 0.9em;

        @supports not (backdrop-filter:blur(30px)) {
            background-color:rgba($white, 0.9);
        }

        @media (prefers-color-scheme: dark) {
            background-color:rgba($midnight, 0.6);

            @supports not (backdrop-filter:blur(30px)) {
                background-color:rgba($midnight, 0.9);
            }
        }
    }

    .footer-list {
        display: flex;
        justify-content: flex-end;
    }

    .footer-list-item {
        padding: 0 0.5em;
    }

    .postface {
        margin-right: auto;
    }

    button {
        margin: 1em auto;
        min-width: 200px;
        padding: 10px 0;
        font-family: $inconsolata;
        font-size: 1em;
        border: none;
        background-color: $emerald;
        color: white;
        border-radius: 3px;
        cursor: pointer;

        &:hover {
            background-color: $malachite;
        }

        &:active {
            background-color: $darkorange;
        }

        &:disabled {
            background-color: $lightgrey;
            cursor: unset;
        }
    }
</style>
