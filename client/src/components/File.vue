<template>
    <li v-bind:style="{ width: file.width + 'px', height: file.height + 'px' }">
        <img v-bind:src="thumbnailUrl" class="file-preview" v-on:click="toggleExpando(file)" />
        <div class="overlay"></div>
        <a class="file-action action-copy" v-on:click="copyUrl" title="Copy the file URL">
            <input type="text" class="file-url" v-model="fullyQualifiedFileUrl" ref="fullyQualifiedUrlElement">
            <img src="./../assets/copy-icon.png" alt="Copy File" class="file-action-icon">
        </a>
        <a class="file-action action-open" v-bind:href="fileUrl" target="_blank" title="Open the file in a new tab">
            <img src="./../assets/open-icon.png" alt="Open File" class="file-action-icon">
        </a>
        <a class="file-action action-details" v-on:click="toggleExpando" title="Show detailed options for this file">
            <img src="./../assets/details-icon.png" alt="Show File Details" class="file-action-icon">
        </a>
    </li>
</template>

<script lang="ts">
    import {Component, Prop, Vue} from 'vue-property-decorator';
    import EventBus from '../EventBus';
    import FileDisplayData from '@/interfaces/FileDisplayData';
    import EnvironmentService from '@/services/EnvironmentService';

    @Component
    export default class FileComponent extends Vue {
        @Prop() public file!: FileDisplayData;

        public $refs!: {
            fullyQualifiedUrlElement: HTMLInputElement
        };

        public env: string = EnvironmentService.getEnv;

        /**
         * The fully qualified URL to a file.
         */
        public get fileUrl(): string {
            return '/' + this.file.file.url;
        }

        /**
         * The fully qualified URL to the thumbnail.
         */
        public get thumbnailUrl(): string {
            if (this.file.file.hasThumbnail) {
                return '/thumbs/default/' + this.file.file.url;
            }
            return this.fileUrl;
        }

        public get fullyQualifiedFileUrl(): string {
            if (EnvironmentService.isProduction) {
                return 'https://fs.lukeify.com/' + this.file.file.url;
            }
            return 'http://localhost:8081/' +  this.file.file.url;
        }

        /**
         * Emits a copy event across the application for another part of the application to accept.
         */
        public copyUrl(): void {
            EventBus.$emit('copy', this.$refs.fullyQualifiedUrlElement);
        }

        /**
         * Toggles the expando for an image to display a number of options
         * to the user.
         */
        public toggleExpando(file: FileDisplayData): void {
            EventBus.$emit('toggle-expando', file);
        }
    }
</script>

<style lang="scss" scoped>
        li {
        overflow:hidden;
        margin:0 5px 5px 5px;
        position:relative;
        cursor:pointer;

        .overlay {
            position:absolute;
            top:0;
            left:0;
            width:100%;
            height:100%;
            transition: all 0.3s ease-in-out;
        }

        .file-action {
            opacity:0;
            display:block;
            position:absolute;

            &.action-copy {
                width:100%;
                height:50%;
                top:0;
                left:0;
            }

            &.action-open {
                width:50%;
                height:50%;
                left:0;
                top:50%;
            }

            &.action-details {
                width:50%;
                height:50%;
                left:50%;
                top:50%;
            }

            &:active .file-action-icon {
                transform:scale(0.8);
            }
        }

        .file-action-icon {
            opacity:0;
            height:32px;
            width:32px;
            position:absolute;
            top:calc(50% - 16px);
            left:calc(50% - 16px);
            transition:opacity 0.3s ease-in-out, transform 0.1s ease-in-out;
        }

        &:hover {
            .overlay {
                -webkit-backdrop-filter:blur(8px);
                background-color:rgba(black, 0.3);
            }

            .file-action, .file-action:hover .file-action-icon {
                opacity:1;
             }

            .file-action-icon {
                opacity:0.5;
            }
        }
    }

    .file-preview {
        display: block;
        max-width:100%;
        max-height:100%;
    }

    .file-url {
        opacity:0;
    }
</style>