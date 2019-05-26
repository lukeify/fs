<template>
    <div>
        <p v-if="!authenticated" class="statement">Personal cloud-based file system and storage. Enter your application key to authenticate.</p>

        <form v-if="!authenticated" v-on:submit.prevent>
            <input class="key-entry"
                   name="key-entry"
                   type="text"
                   ref="keyEntryInput"
                   v-model="key"
                   autocomplete="off"
                   v-on:keyup="authenticationHandler"
                   placeholder="key" />
        </form>
    </div>
</template>
<script lang="ts">
    import { Component, Vue } from 'vue-property-decorator';
    import {debounce} from '@/misc/helpers';

    @Component
    export default class AuthComponent extends Vue {
        /**
         * The key used to authenticate with the backend server.
         */
        public key: string = '';

        /**
         * Whether the application has authenticated.
         */
        public authenticated: boolean = false;

        public defaultColorString = '#aaaaaa';

        public successColorString = 'rgb(80, 200, 120)';

        public errorColorString = 'rgb(239, 104, 51)';

        /**
         * The Id associated with the authentication UI setInterval.
         */
        public authenticationUiIntervalId!: number|null;

        /**
         * The Id associated with the authentication UI setTimeout.
         */
        public authenticationUiTimeoutId!: number|null;

        public debouncedFn!: () => any;

        /**
         * HTML document references.
         */
        public $refs!: {
            keyEntryInput: HTMLElement,
        };

        /**
         * When the component is mounted, check to see if an item is stored in localStorage that represents
         * the app key. If there is, retrieve it and automatically authenticate.
         */
        public mounted(): void {
            if (localStorage.getItem('appKey') !== null) {
                this.key = localStorage.getItem('appKey') as string;
                this.authenticate();
            }
            this.debouncedFn = debounce(this.authenticate, 500);
        }

        /**
         * Debounces authentication requests as needed.
         */
        public authenticationHandler(): void {
            this.debouncedFn();
        }

        /**
         * Attempts to perform authentication.
         */
        public authenticate(): void {
            const self = this;

            if (this.key !== '') {
                fetch('/api/auth/validate', {
                    method: 'POST',
                    body: '"' + this.key + '"',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                }).then(async (response) => {
                    if (response.status === 204) {
                        await this.flashKeyField(this.successColorString);
                        self.$emit('authenticate');
                        localStorage.setItem('appKey', self.key);
                    } else {
                        await this.flashKeyField(this.errorColorString);
                    }

                });
            } else {
                self.$refs.keyEntryInput.style.borderBottomColor = this.defaultColorString;
            }
        }

        /**
         * After receiving the success or failure outcome of the authentication attempt, flash the border bottom
         * color to indicate success or failure.
         *
         * @param colorToFlash - The color the field should be flashed.
         */
        public flashKeyField(colorToFlash: string): any {
            const self = this;
            let index = 0;
            const flashCount = 6;
            const flashInterval = 130;

            return new Promise((resolve, reject) => {
                self.authenticationUiIntervalId = window.setInterval(() => {
                    if (index < flashCount) {
                        const style = self.$refs.keyEntryInput.style.borderBottomColor; // window.getComputedStyle
                        if (style !== colorToFlash) {
                            self.$refs.keyEntryInput.style.borderBottomColor = colorToFlash;
                        } else {
                            self.$refs.keyEntryInput.style.borderBottomColor = self.defaultColorString;
                        }
                        index++;
                    } else {
                        if (self.authenticationUiIntervalId) {
                            window.clearInterval(self.authenticationUiIntervalId);
                            self.authenticationUiIntervalId = null;
                        }
                        self.$refs.keyEntryInput.style.borderBottomColor = colorToFlash;
                        self.authenticationUiTimeoutId = window.setTimeout(() => {
                            self.$refs.keyEntryInput.style.borderBottomColor = self.defaultColorString;
                            self.authenticationUiTimeoutId = null;
                            resolve();

                        }, 500);
                    }
                }, flashInterval);
            });
        }
    }
</script>
<style lang="scss" scoped>
    @import "../styles/design.scss";

    .statement {
        text-align: center;
        margin: 1em;
    }

    form {
        text-align: center;
    }

    .key-entry {
        background-color: transparent;
        border: none;
        border-bottom: 3px solid $lightgrey;
        font-size: 1em;
        margin: 4em 0;
        padding: 4px 10px;
        font-family: $inconsolata;
        width: 320px;
        color:$grey;

        @media (prefers-color-scheme: dark) {
            color: $lightgrey;
        }

        &:focus {
            outline:none;
        }
    }
</style>
