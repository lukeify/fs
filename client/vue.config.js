module.exports = {
    devServer: {
        proxy: {
            '/api': {
                target: 'http://localhost:5000',
                secure: false,
                changeOrigin: true
            },
            '/images': {
                target: 'http://localhost:5000',
                secure: false,
                pathRewrite: {
                    "^/images": ""
                },
                changeOrigin: true
            },
        }
    },
    configureWebpack: {
        output: {
            filename: '[name].[hash].js'
        }
    },
};
