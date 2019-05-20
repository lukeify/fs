type DebouncedFn = (...args: any[]) => any;

export function debounce(func: DebouncedFn, wait: number, immediate: boolean = false): () => any {
    let timeout: number|undefined;
    return function(this: any) {
        const self = this;
        const args: IArguments = arguments;
        const doLater = () => {
            timeout = undefined;
            if (!immediate) {
                func.apply(self, Array.from(args));
            }
        };
        const shouldCallNow = immediate && !timeout;
        window.clearTimeout(timeout); // tslint:disable-line
        timeout = window.setTimeout(doLater, wait);
        if (shouldCallNow) {
            func.apply(self, Array.from(args));
        }
    };
}
