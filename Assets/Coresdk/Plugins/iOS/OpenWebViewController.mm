//
//  OpenWebViewController.mm
//
//  Created by 919575700@qq.com on 4/30/16.
//  Copyright (c) 2015 Jiangxh. All rights reserved.
//
#import "OpenWebViewController.h"
#import <WebKit/WebKit.h>

extern "C" {
    typedef void CallbackT(const char *url);
    
    CallbackT *onDeepLinkActivatedCallback;
    
    void openURL(const char *url, CallbackT callback)
    {
        onDeepLinkActivatedCallback = callback;
        
        //獲取Unity rootviewcontroller
        UIViewController *unityRootVC = UnityGetGLViewController();
        UIView *unityView = UnityGetGLView();
        //建立我們需要開啟的webview
        OpenWebViewController *webVC = [[OpenWebViewController alloc]init];
        //傳入需要開啟的網址
        webVC.url = [NSString stringWithUTF8String:url];
        //新增一個自定義導航檢視把將要開啟的webview作為根檢視
        UINavigationController *navVC = [[UINavigationController alloc]initWithRootViewController:webVC];
        //新增到Unity場景的rootview
        [unityRootVC addChildViewController:navVC];
        [unityView addSubview:navVC.view];
    }
}

@interface OpenWebViewController ()<WKNavigationDelegate>

/**
 *  webview
 */
@property (nonatomic, strong)WKWebView *webView;

/**
 *  載入網址頁面
 */
- (void)loadWebViewWithString: (NSString *)urlStr;

@end

@implementation OpenWebViewController

/**
 *  檢視載入
 */
- (void)viewDidLoad {
    [super viewDidLoad];
    
    //背景色
    self.view.backgroundColor = [UIColor grayColor];
    //關閉上方Title Bar
    self.navigationController.navigationBarHidden = YES;
    
    //螢幕尺寸
    CGFloat ApplicationW = [[UIScreen mainScreen] bounds].size.width;
    CGFloat ApplicationH = [[UIScreen mainScreen] bounds].size.height;
    
    // webview
    _webView = [[WKWebView alloc] init];
    //frame
    [_webView setFrame:CGRectMake(2, 2, ApplicationW, ApplicationH - 2)];
    //自適應
    _webView.autoresizingMask = UIViewAutoresizingFlexibleHeight |UIViewAutoresizingFlexibleWidth;
    //設定webview的代理
    _webView.navigationDelegate= self;
    //新增到頁面
    [self.view addSubview:_webView];
    //載入網頁
    [self loadWebViewWithString:_url];
}

/**
 *  載入網址頁面
 */
- (void)loadWebViewWithString:(NSString*)urlStr {
    // 網址字串轉URL
    NSURL *url = [NSURL URLWithString:urlStr];
    // url請求
    NSURLRequest *request = [NSURLRequest requestWithURL:url];
    // 載入url請求
    [_webView loadRequest:request];
}

/**
 *  關閉webview頁面
 */
- (void)CloseWebView {
    [self.webView removeFromSuperview];
    [self setWebView:nil];
	
    [self.navigationController.view removeFromSuperview];
    [self.navigationController dismissViewControllerAnimated:YES completion:nil];
}

#pragma mark webview委託方法
- (void)webView:(WKWebView *)webView decidePolicyForNavigationAction:(WKNavigationAction *)navigationAction decisionHandler:(void (^)(WKNavigationActionPolicy))decisionHandler
{
    // NSLog(@"請求前會先進入這個方法  webView:decidePolicyForNavigationActiondecisionHandler: %@   \n\n  ",navigationAction.request);
    
    NSString *URLString = [navigationAction.request.URL absoluteString];
    if ([URLString hasPrefix:@"coresdk://"]) {
        onDeepLinkActivatedCallback([URLString UTF8String]);
        decisionHandler(WKNavigationActionPolicyCancel);
        
        [self CloseWebView];
    }
    else {
        decisionHandler(WKNavigationActionPolicyAllow);
    }
}

@end
