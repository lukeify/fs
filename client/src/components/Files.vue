<template>
    <div id="files-vue" ref="filesContainerRef">
        <h2 v-if="fileCount > 0"><span>{{ fileCount }} </span>{{ fileCount !== 1 ? "files" : "file" }} <span>({{ humanReadableStorageSpaceUsed }})</span></h2>
        <h2 v-if="fileCount === 0">No files</h2>

        <ul id="files">
            <file-component v-for="file in fileDisplayData" v-bind:key="file.index" v-bind:file="file" v-on:toggle-expando="toggleExpando"></file-component>
        </ul>

        <p id="no-files" v-if="fileCount === 0">Nothing here 😐</p>

        <button v-if="!hasReachedEnd" v-on:click="fetch">More</button>
    </div>
</template>

<script lang="ts">
import {Component, Prop, Vue, Watch} from 'vue-property-decorator';
import EventBus from '../EventBus';

import FileComponent from '@/components/File.vue';
import File from '@/interfaces/File';
import ImageLayoutService from '../services/ImageLayoutService';
import {FilesystemData} from '@/interfaces/FilesystemData';
import FileDisplayData from '@/interfaces/FileDisplayData';

/**
 * Handles the files.
 */
@Component({
    components: {
        'file-component': FileComponent,
    },
})
export default class FilesComponent extends Vue {

    public $refs!: {
        filesContainerRef: HTMLElement,
    };

    /**
     * Any recently uploaded files.
     */
    @Prop() public recentlyUploadedFiles!: File[];

    /**
     * The number of files in the application.
     */
   public fileCount: number = 0;

   /**
    * The total storage space used in bytes.
    */
   public storageSpaceUsed: number = 0;

    /**
     * The total storage space used by all files in the application, as a human readable string.
     */
    public humanReadableStorageSpaceUsed: string = '0B';

    /**
     * All the files that have been requested by the client.
     */
    public allFiles: File[] = [];

    /**
     * The file display data used to format and wrap each file for display in the UI.
     */
    public fileDisplayData: FileDisplayData[] = [];

    /**
     * Have we reached the end of the file list?
     */
    public hasReachedEnd: boolean = true;

    /**
     * The number of files to retrieve in any single request.
     */
    public paginate: number = 100;

    /**
     * Whether there's an expando present.
     */
    public expando: any = null;

    /**
     * Listen for global events propagated across the application.
     */
    public created(): void {
        EventBus.$on('wipeComplete', () => {
            this.storageSpaceUsed = 0;
            this.humanReadableStorageSpaceUsed = this.computeStorageSpaceUsed(0);

            this.fileCount = 0;
            this.allFiles = [];
            this.fileDisplayData = [];
        });
    }

    /**
     * When the component is created, make a request to retrieve all file metadata,
     * for statistics, etc; and also a pagination of the first n results.
     */
    public mounted(): void {
        this.humanReadableStorageSpaceUsed = this.computeStorageSpaceUsed(0);

        // Create a request to retrieve the metadata for the filesystem.
        fetch('/api/files/meta', { method: 'GET' })
            .then((res) => res.json())
            .then((data: FilesystemData) => {
                this.fileCount = data.count;
                this.storageSpaceUsed = data.bytesUsed;
                this.humanReadableStorageSpaceUsed = this.computeStorageSpaceUsed(data.bytesUsed);
                this.setDocumentTitle(this.fileCount, this.humanReadableStorageSpaceUsed);
        });

        // Create a request to retrieve the first pagination of results.
        fetch('/api/files?paginate=' + this.paginate, { method: 'GET' })
            .then((res) => res.json())
            .then((res: File[]) => {
                this.allFiles = res;

                const imageLayoutService = new ImageLayoutService();

                this.fileDisplayData = imageLayoutService.buildRows(this.allFiles, {
                    rowWidth: this.computeDesiredRowWidth(),
                    desiredItemHeight: 200,
                    itemSpacing: 10,
                });
        });
    }

    /**
     * Compute the storage spaced used by converting the value of bytes into an actual
     * united, understandable metric.
     *
     * @param bytesUsed - the number of bytes used in the filesystem.
     *
     * @returns A string containing the storage space used.
     */
    public computeStorageSpaceUsed(bytesUsed: number): string {
        if (bytesUsed === null || bytesUsed === 0) {
            return '0B';
        }

        const suffixes = ['B', 'KB', 'MB', 'GB', 'TB', 'PB'];
        for (const suffix of suffixes) {
            if (bytesUsed < (Math.pow(1024, suffixes.indexOf(suffix) + 1))) {
                const spaceUsed: number = bytesUsed / Math.pow(1024, suffixes.indexOf(suffix));

                if (suffix !== 'B' && spaceUsed % 1 !== 0) {
                    return spaceUsed.toFixed(2) + suffix;
                }
                return spaceUsed + suffix;
            }
        }

        return 'Unknown';
    }

    /**
     * Given the viewport size, compute the desired row width, taking into account the margin present on the root
     * HTML element. This function is also called when a resize event occurs.
     */
    public computeDesiredRowWidth(): number {
        return this.$refs.filesContainerRef.getBoundingClientRect().width;
    }

    /**
     * Fetch new files.
     */
    public fetch(): void {
        const currentFilePosition = this.allFiles.length;

        fetch('/api/files?from=' + currentFilePosition + '&paginate=' + this.paginate)
            .then((res: Response) => res.json())
            .then((data) => {
            this.hasReachedEnd = data.files.length < this.paginate;
            this.allFiles = this.allFiles.concat(data.files);

            const imageLayoutService = new ImageLayoutService();
            this.fileDisplayData = imageLayoutService.buildRows(this.allFiles, {
                rowWidth: 1000,
                desiredItemHeight: 200,
                itemSpacing: 10,
            });

        }).catch((err: any) => {
            // non-empty block
        });
    }

    @Watch('recentlyUploadedFiles')
    public onUploadSuccessFn(files: File[]): void {
        this.allFiles.unshift(...files);
        this.fileCount          += files.length;
        this.storageSpaceUsed   += files.reduce((a, v, i) => a += v.filesize, 0);
        this.humanReadableStorageSpaceUsed = this.computeStorageSpaceUsed(this.storageSpaceUsed);
        this.setDocumentTitle(this.fileCount, this.humanReadableStorageSpaceUsed);

        const imageLayoutService = new ImageLayoutService();
        this.fileDisplayData = imageLayoutService.buildRows(this.allFiles, {
            rowWidth: 1000,
            desiredItemHeight: 200,
            itemSpacing: 10,
        });

    }

    public setDocumentTitle(files: number, storageSpace: string): void {
        if (files === 0) {
            document.title = `No files | fs.lukeify.com`;
        } else if (files === 1) {
            document.title = `1 file, ${storageSpace} | fs.lukeify.com`;
        } else {
            document.title = `${files} files, ${storageSpace} | fs.lukeify.com`;
        }
    }

    /**
     *
     */
    public toggleExpando(): void {
        // non-empty block.
    }
}
</script>

<style lang="scss" scoped>
    #files-vue {
        text-align: left;
    }

    #no-files {
        font-size: 1.2em;
        text-align: center;
    }

    #files {
    }

    file, li {
        display: inline-block;
    }
</style>
