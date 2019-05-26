<template>
    <div>
        <dropzone id="dropzone" ref="dropzone" v-bind:options="opts"
                  v-on:vdropzone-file-added="onDropzoneFileAdded" v-on:vdropzone-sending-multiple="onDropzoneSendingMultiple" v-on:vdropzone-complete-multiple="removeFiles"
                  v-on:vdropzone-success-multiple="onDropzoneSuccessMultiple" v-on:vdropzone-error-multiple="onDropzoneErrorMultiple">
        </dropzone>

        <files v-bind:recentlyUploadedFiles="recentlyUploadedFiles"></files>
    </div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import FilesComponent from '@/components/Files.vue';
import {DropzoneOptions} from 'dropzone';
const vue2Dropzone = require('vue2-dropzone'); // tslint:disable-line

@Component({
    components: {
        dropzone: vue2Dropzone, files: FilesComponent,
    },
})
export default class HomeComponent extends Vue {

    public recentlyUploadedFiles: any[] = [];

    /**
     *
     */
    public opts: DropzoneOptions = {
        url: '/api/files',
        method: 'put',
        withCredentials: true,
        uploadMultiple: true,
        maxFiles: 10,
        clickable: true,
        thumbnailHeight: 200,
        autoProcessQueue: true,
        dictDefaultMessage: 'Drop anything here',
        paramName: 'files',
    };

    public $refs!: {
        dropzone: any,
    };

    /**
     * Whether the upload form is currently disabled.
     */
    public isUploadDisabled: boolean = false;

    /**
     * Authorization Header to insert the key at.
     */
    public authorizationHeaderKey = 'Authorization';
    public authorizationHeaderValuePrefix = 'Bearer ';

    /**
     *
     */
    public removeFiles(): void {
        this.$refs.dropzone.removeAllFiles();
    }

    /**
     * When a file is added to the dropzone, we want to disable uploads to the server.
     */
    public onDropzoneFileAdded(): void {
        this.isUploadDisabled = false;
    }

    /**
     * When dropzone is told to upload files, this function will be called. This
     * is where we intercept its XHR request and add on the Authorization header
     * to allow us to be accepted by the server.
     *
     * @param files -
     * @param xhr - The XHR which is being sent to the server.
     * @param formData - The formData associated with the XHR.
     */
    public onDropzoneSendingMultiple(files: any, xhr: any, formData: any): void {
        xhr.setRequestHeader(
            this.authorizationHeaderKey,
            this.authorizationHeaderValuePrefix + localStorage.getItem('appKey'),
        );
    }

    /**
     * Called when dropzone reports a success with uploading multiple files.
     */
    public onDropzoneSuccessMultiple(file: any, serverResponse: File[]): void {
        this.recentlyUploadedFiles = serverResponse;
    }

    /**
     * Called when dropzone reports errors with uploading multiple files.
     */
    public onDropzoneErrorMultiple(): void {
        // non-empty block
    }
}
</script>

<style lang="scss">
    @import "../styles/design.scss";

    $dz-preview-margin:10px;
    $dz-zone-size:200px;
    $dz-preview-size:100px;
    $dz-padding:20px;

    $dot-size: 2px;
    $dot-space: 10px;

    #dropzone.vue-dropzone {
        min-height:$dz-zone-size;
        cursor:pointer;
        display:flex;
        flex-direction:column;
        justify-content:center;
        align-items: center;

        @media (max-height: #{$vertical-breakpoint}) {
            min-height:$dz-zone-size * 0.75;
        }

        @media (prefers-color-scheme: light) {
            background: linear-gradient(90deg, $white ($dot-space - $dot-size), transparent 1%) center,
            linear-gradient($white ($dot-space - $dot-size), transparent 1%) center,
            darken($white, 20%);
            background-size: $dot-space $dot-space;
        }

        @media (prefers-color-scheme: dark) {
            background: linear-gradient(90deg, $midnight ($dot-space - $dot-size), transparent 1%) center,
            linear-gradient($midnight ($dot-space - $dot-size), transparent 1%) center,
            lighten($midnight, 15%);
            background-size: $dot-space $dot-space;
        }

        .dz-message {
            opacity:0.8;
            font-size:larger;
        }

        &.dz-started .dz-message {
            display:none;
        }

        .dz-preview {
            margin:$dz-preview-margin;

            .dz-image img {
                max-width:$dz-preview-size;
                max-height:$dz-preview-size;
            }

            .dz-details {
                background-color:transparent;
            }

            &:not(.dz-processing) .dz-progress {
                animation:none;
            }

            .dz-progress {
                height:10px;
                left:0;
                bottom:0;
                top:auto;
                width:$dz-preview-size;
                background:none;
                margin:0;
                border-radius:0;

                .dz-upload {
                    background-color:$malachite;
                }
            }
        }
    }
</style>
