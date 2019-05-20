export default interface File {
    name: string;
    url: string;
    mimetype: string;
    extension: string;
    width: number|null;
    height: number|null;
    filesize: number;
    views: number;
    createdAt: string;
    updatedAt: string;
    hasThumbnail: boolean;
}
