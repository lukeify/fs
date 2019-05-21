module.exports = {
    devServer: {
        proxy: {
            '/api': {
                target: 'http://localhost:5000',
                secure: false,
                changeOrigin: true
            },
            '/': {
                target: 'http://localhost:5000',
                secure: false,
                pathRewrite: {
                    "^/": ""
                },
                ws: false,
                changeOrigin: true
            },
        }
    },
    configureWebpack: {
        output: {
            filename: '[name].[hash].js'
        }
    },
    outputDir: '../wwwroot'
};
