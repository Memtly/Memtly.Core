const path = require('path');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const webpack = require('webpack');
const glob = require('glob');
const CopyPlugin = require('copy-webpack-plugin');
const { WebpackManifestPlugin } = require('webpack-manifest-plugin');

const themeEntries = glob.sync(`${path.resolve(__dirname, 'src/themes')}/*.css`).reduce((acc, filePath) => {
    const themeName = path.basename(filePath, '.css');
    acc[`themes/${themeName}`] = path.resolve(__dirname, `src/themes/${themeName}.css`);
    return acc;
}, {});

module.exports = (env, argv) => {
    const isProduction = argv.mode === 'production';

    return {
        entry: {
            main: path.resolve(__dirname, 'src/main.js'),
            ...themeEntries
        },
        resolve: {
            alias: {
                '@': path.resolve(__dirname, 'src'),
                '@pages': path.resolve(__dirname, 'src/pages'),
                '@modules': path.resolve(__dirname, 'src/modules'),
                '@utilities': path.resolve(__dirname, 'src/modules/utilities'),
                '@validation': path.resolve(__dirname, 'src/modules/validation'),
                '@themes': path.resolve(__dirname, 'src/themes'),
                '@styles': path.resolve(__dirname, 'src/css'),
                '@images': path.resolve(__dirname, 'src/images'),
            }
        },
        output: {
            path: path.resolve(__dirname, 'wwwroot/dist'),
            filename: isProduction ? '[name].[contenthash:8].js' : '[name].js',
            publicPath: '/_content/Memtly.Core/dist/',
            clean: {
                keep: /fonts\/|images\//
            }
        },
        module: {
            rules: [
                {
                    test: /\.js$/,
                    exclude: /node_modules/,
                    use: {
                        loader: 'babel-loader',
                        options: {
                            presets: ['@babel/preset-env']
                        }
                    }
                },
                {
                    test: /\.css$/,
                    use: [
                        MiniCssExtractPlugin.loader,
                        'css-loader'
                    ]
                },
                {
                    test: /\.(woff|woff2|eot|ttf|otf)$/,
                    type: 'asset/resource',
                    generator: {
                        filename: 'fonts/[name][ext]'
                    }
                },
                {
                    test: /\.(svg|png|jpg|jpeg|gif)$/,
                    type: 'asset/resource',
                    generator: {
                        filename: 'images/[name][ext]'
                    }
                }
            ]
        },
        plugins: [
            new MiniCssExtractPlugin({
                filename: isProduction ? '[name].[contenthash:8].css' : '[name].css'
            }),
            new webpack.ProvidePlugin({
                $: 'jquery',
                jQuery: 'jquery',
                'window.jQuery': 'jquery',
                Popper: ['@popperjs/core', 'default']
            }),
            new CopyPlugin({
                patterns: [
                    {
                        from: 'node_modules/@fortawesome/fontawesome-free/webfonts',
                        to: 'fonts'
                    }
                ]
            }),
            new WebpackManifestPlugin({
                fileName: 'manifest.json',
                publicPath: '/_content/Memtly.Core/dist/'
            })
        ],
        optimization: {
            runtimeChunk: 'single'
        }
    };
};