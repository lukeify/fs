import ImageLayoutOpts from '@/interfaces/ImageLayoutOpts';
import File from '@/interfaces/File';
import FileDisplayData from '@/interfaces/FileDisplayData';

type RowAlterationSuggestion = 'includeLastFile' | 'excludeLastFile';

export default class ImageLayoutService {

    private files!: File[];

    /**
     * The rows that will eventually be returned.
     */
    private rows: FileDisplayData[][] = [];

    /**
     * The current row being worked on.
     */
    private currentRow!: FileDisplayData[];

    /**
     * The options associated with the image layout.
     */
    private options!: ImageLayoutOpts;

    /**
     * Given an array of files, and image layout options, build the rows
     * that will be displayed on screen.
     *
     * @param files - An array of files that need to be laid out.
     * @param options - The options that should be used to generate a layout.
     *
     * @returns An array of rows representing FileDisplay Data.
     */
    public buildRows(files: File[], options: ImageLayoutOpts): FileDisplayData[] {
        this.files = files;
        this.options = options;
        this.currentRow = [];

        let rowWidthRemaining = options.rowWidth;
        // let actualItemHeight = 0;

        for (let index = 0; index < this.files.length; index++) {
            const desiredItemWidth: number = options.desiredItemHeight * this.getAspectRatio(this.files[index]);
            rowWidthRemaining -= (desiredItemWidth + options.itemSpacing);

            this.currentRow.push({
                index,
                file: this.files[index],
                width: desiredItemWidth,
                height: options.desiredItemHeight,
            });

            // If there is no row width remaining, everything is perfect
            if (rowWidthRemaining === 0) {
                this.rows.push(this.currentRow);
                this.currentRow = [];
                continue;
            }

            // If the row has a negative row width remaining value, it's time to optimize.
            // Determine if it's best to include the last image and make the row height smaller
            // or exclude the last image and make the row height larger.
            if (rowWidthRemaining < 0) {
                const includeLastFile = this.computeRowWidth(this.currentRow, false);
                const excludeLastFile = this.computeRowWidth(this.currentRow, true);

                if (this.bestFit(this.options.rowWidth, includeLastFile, excludeLastFile) === 'excludeLastFile') {
                    this.currentRow.pop();
                    index -= 1;
                }

                const scaleFactor = this.calculateRowScaleFactor(this.currentRow);
                this.currentRow = this.rescaleRow(this.currentRow, scaleFactor);

                this.rows.push(this.currentRow);
                this.currentRow = [];
                rowWidthRemaining = this.options.rowWidth;
            }

            // If we are dealing with the last image in the array, simply append the last item to the last row.
            if (index === this.files.length - 1) {
                this.rows.push(this.currentRow);
                this.currentRow = [];
            }
        }

        return this.rows.flat();
    }

    /**
     * Retrieve the aspect ratio of a given file. If the file is not an image, 1 will be returned as we utilise
     * squares for display purposes.
     *
     * @params file - The file to retrieve the aspect ratio for.
     *
     * @returns The aspect ratio of the file.
     */
    private getAspectRatio(file: File): number {
        if (file.width && file.height) {
            return file.width / file.height;
        }
        return 1;
    }

    /**
     * Computes the row width for an array of FileDisplayData, returning the width in pixels.
     *
     * @param row - The row to calculate a width for.
     * @param excludeLastImage - Whether the computed row width should include the last image. Used to evaluate whether
     * it's better to include the image in this row, or pop it off and push it onto the new row.
     *
     * @returns The total width of the row.
     */
    private computeRowWidth(row: FileDisplayData[], excludeLastImage: boolean = false): number {
        if (excludeLastImage) {
            return row.slice(0, row.length - 1)
                .map((rd: FileDisplayData) => rd.width)
                .reduce((a: number, v: number) => a += v, 0) + ((row.length - 1) * this.options.itemSpacing);
        }

        return row.map((rd: FileDisplayData) => rd.width)
            .reduce((a: number, v: number) => a += v, 0) + (row.length * this.options.itemSpacing);
    }

    /**
     * Determine, given the desired row width whether the best fit for the row is to include or exclude the last file
     * in the row; and return a suggestion as such on how the program should act.
     *
     * @param rowWidth - The width of the row to try and best meet.
     * @param includeLastFile - The width of the row, when the last file in the row is included.
     * @param excludeLastFile - The width of the row, when the last file in the row is excluded.
     *
     * @return A string representing either a suggestion to include or exclude the last item in the row.
     */
    private bestFit(rowWidth: number, includeLastFile: number, excludeLastFile: number): RowAlterationSuggestion {
        return (includeLastFile - rowWidth) < (rowWidth - excludeLastFile) ? 'includeLastFile' : 'excludeLastFile';
    }

    /**
     * Given a row, calculate the scale factor—upwards or downwards to meet the necessary row width requirements.
     *
     * @param row - The row to calculate the scale factor for, to meet the needed width of the row.
     *
     * @returns The scale factor needed for each image to fit in the row nicely.
     */
    private calculateRowScaleFactor(row: FileDisplayData[]): number {
        const currentRowWidth   = this.computeRowWidth(row);
        const totalSpacingInRow = row.length * this.options.itemSpacing;

        const currentState = currentRowWidth - totalSpacingInRow;
        const desiredState = this.options.rowWidth - totalSpacingInRow;

        /*if (desiredState > currentState) {
            return currentState / desiredState;
        } else {
            return desiredState / currentState;
        }*/
        return desiredState / currentState;
    }

    /**
     * Given a row and a scale factor, correctly scale each image and update its FileDisplayData dimension properties
     * as per the provided scale factor.
     *
     * @param row - The row to rescale.
     * @param scaleFactor - The scale factor to apply to the row.
     *
     * @returns The mutated row with updated dimension parameters.
     */
    private rescaleRow(row: FileDisplayData[], scaleFactor: number): FileDisplayData[] {
        // Calculate the actual item height elements in this row need to be.
        const actualItemHeight = this.options.desiredItemHeight * scaleFactor;

        // Rescale every item width by the scale factor.
        row.forEach((fileDisplayData: FileDisplayData, index: number, arr: FileDisplayData[]) => {
            arr[index].width = fileDisplayData.width * scaleFactor;
            arr[index].height = actualItemHeight;
        });

        return row;
    }
}
