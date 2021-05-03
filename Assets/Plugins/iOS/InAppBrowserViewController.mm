//
//  InAppBrowserViewController.m
//  Unity-iPhone
//
//  Created by Piotr on 04/03/16.
//
//
#include "InAppBrowserViewController.h"
extern void UnitySendMessage(const char *, const char *, const char *);
#define SYSTEM_VERSION_EQUAL_TO(v)                  ([[[UIDevice currentDevice] systemVersion] compare:v options:NSNumericSearch] == NSOrderedSame)

NSString* deviceName()
{
    struct utsname systemInfo;
    uname(&systemInfo);
    
    return [NSString stringWithCString:systemInfo.machine
                              encoding:NSUTF8StringEncoding];
}

static NSString *GameObjectName = @"InAppBrowserBridge";

void SendToUnity(NSString *methodName, NSString *param) {
    UnitySendMessage(GameObjectName.UTF8String, methodName.UTF8String, param.UTF8String);
}

void OnBrowserJSCallback(NSString *callbackMessage) {
    SendToUnity(@"OnBrowserJSCallback", callbackMessage);
}

void OnBrowserFinishedLoading(NSURLRequest *request) {
    SendToUnity(@"OnBrowserFinishedLoading", request.URL.absoluteString);
}

void OnBrowserStartedLoading(NSURLRequest *request) {
    SendToUnity(@"OnBrowserStartedLoading", request.URL.absoluteString);
}

void OnBrowserClosed() {
    SendToUnity(@"OnBrowserClosed", @"");
}

void OnBrowserFinishedLoadingWithError(NSURLRequest *request, NSError *error) {
    SendToUnity(@"OnBrowserFinishedLoadingWithError", [NSString stringWithFormat:@"%@,%@",
                                                       request.URL.absoluteString,
                                                       error.description]);
}

@implementation InAppBrowserConfig

+ (InAppBrowserConfig *)defaultDisplayOptions {
    InAppBrowserConfig *displayOptions = [InAppBrowserConfig new];
    displayOptions.pageTitle = nil;
    displayOptions.displayURLAsPageTitle = YES;
    displayOptions.textColor = nil;
    displayOptions.barBackgroundColor = nil;
    displayOptions.backButtonText = @"Back";
    displayOptions.pinchAndZoomEnabled = NO;
    return displayOptions;
}

@end

@implementation InAppBrowserViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    
    if (!_config) {
        _config = [InAppBrowserConfig defaultDisplayOptions];
    }
    
    UIWebView *webView = [UIWebView new];
    // https://github.com/lionheart/openradar-mirror/issues/16684
    [self configureWebViewForIphone5CBug:webView];
    
    if (_config.browserBackgroundColor) {
        webView.backgroundColor = _config.browserBackgroundColor;
        webView.opaque = NO;
        self.navigationController.navigationBar.translucent = NO;
    }
    
    self.webView = webView;
    [self.view addSubview:webView];
    [self configureNavigationBar];
    [webView setTranslatesAutoresizingMaskIntoConstraints:NO];
    [self.view addConstraints:[NSLayoutConstraint
                               constraintsWithVisualFormat:@"H:|-0-[webView]-0-|"
                               options:NSLayoutFormatDirectionLeadingToTrailing
                               metrics:nil
                               views:NSDictionaryOfVariableBindings(webView)]];
    
    [self.view addConstraints:[NSLayoutConstraint
                               constraintsWithVisualFormat:@"V:|-0-[webView]-0-|"
                               options:NSLayoutFormatDirectionLeadingToTrailing
                               metrics:nil
                               views:NSDictionaryOfVariableBindings(webView)]
     ];
    
    if (_config.pinchAndZoomEnabled) {
        webView.scalesPageToFit = YES;
    }
    [self startLoadingWebView];
}

- (void)viewDidAppear:(BOOL)animated {
    [super viewDidAppear:animated];
    if (_config.shouldUsePlaybackCategory) {
        [self switchToPlaybackCategory];
    }
    
}

- (void)viewDidDisappear:(BOOL)animated {
    [super viewDidDisappear:animated];
    if (_config.shouldUsePlaybackCategory) {
        [self switchToInitialCategory];
    }

}

// http://www.kokosoft.pl/forums/topic/iphone-5c-video-playback-issue/
- (void)configureWebViewForIphone5CBug:(UIWebView *)webView {
    BOOL isOn102 = SYSTEM_VERSION_EQUAL_TO(@"10.2");
    NSString *device = deviceName();
    BOOL isOnIphone5C = [device isEqualToString:@"iPhone5,3"] || [device isEqualToString:@"iPhone5,4"];
    
    if (isOn102 && isOnIphone5C) {
        [webView setMediaPlaybackRequiresUserAction:NO];
    }
}

- (void)switchToPlaybackCategory {
    self.initialCategory = [AVAudioSession sharedInstance].category;
    NSError *error;
    BOOL succeeded = [[AVAudioSession sharedInstance]
                      setCategory:AVAudioSessionCategoryPlayback
                      error:&error];
    if (!succeeded) {
        // TODO: Handle the error
        self.initialCategory = nil;
    }
    else {
        // Play the video or audio file
    }
}

- (void)switchToInitialCategory {
    NSError *error;
    BOOL succeeded = [[AVAudioSession sharedInstance]
                      setCategory:self.initialCategory
                      error:&error];
    if (!succeeded) {
        // Handle the error
    }
}

- (void)sendJSMessage:(NSString *)message {
    [_webView stringByEvaluatingJavaScriptFromString:message];
}

- (void)startLoadingWebView {
    _webView.delegate = self;
    
    if (_HTML) {
        [_webView loadHTMLString:_HTML baseURL:nil];
    } else {
        NSURLRequest *request = [NSURLRequest requestWithURL:[NSURL URLWithString:_URL]];
        [_webView loadRequest: request];
    }
    
    UIActivityIndicatorView *indicator = [[UIActivityIndicatorView alloc] initWithActivityIndicatorStyle:UIActivityIndicatorViewStyleGray];
    _indicatorView.hidesWhenStopped  = YES;
    
    if (_config.loadingIndicatorColor) {
        indicator.color = _config.loadingIndicatorColor;
    }
    
    self.indicatorView = indicator;
    if (!_config.hidesDefaultSpinner) {
        [self.view addSubview:_indicatorView];
        [_indicatorView setTranslatesAutoresizingMaskIntoConstraints:NO];
        [self.view addConstraints:@[[NSLayoutConstraint constraintWithItem:_indicatorView
                                                                 attribute:NSLayoutAttributeCenterX
                                                                 relatedBy:NSLayoutRelationEqual
                                                                    toItem:self.view
                                                                 attribute:NSLayoutAttributeCenterX
                                                                multiplier:1.f constant:0.f],
                                    [NSLayoutConstraint constraintWithItem:_indicatorView
                                                                 attribute:NSLayoutAttributeCenterY
                                                                 relatedBy:NSLayoutRelationEqual
                                                                    toItem:self.view
                                                                 attribute:NSLayoutAttributeCenterY
                                                                multiplier:1.f constant:0.f]
                                    ]
         
         ];
    }

    [_indicatorView startAnimating];
}

- (void)configureNavigationBar {
    
    if (_config.hidesTopBar) {
        self.navigationController.navigationBarHidden = YES;
        return;
    }
    
    UIBarButtonItem* barButton = [[UIBarButtonItem alloc] initWithTitle:_config.backButtonText
                                                                  style:UIBarButtonItemStylePlain
                                                                 target:self
                                                                 action:@selector(backButtonPressed)];
    
    
    
    if (_config.backButtonLeftRightMargin) {
        [barButton setTitlePositionAdjustment:UIOffsetMake(CGFloat(_config.backButtonLeftRightMargin.floatValue), 0) forBarMetrics:UIBarMetricsDefault];
    }
    
    NSDictionary *backButtonAttrs = [self createBackButtonTitleAttributes];
    if (backButtonAttrs) {
        [barButton setTitleTextAttributes:backButtonAttrs forState:UIControlStateNormal];
    }
    
    
    [self.navigationItem setLeftBarButtonItem:barButton];
    
    NSDictionary *titleAttrs = [self createTitleAttributes];
    if (titleAttrs) {
        [self.navigationController.navigationBar setTitleTextAttributes: titleAttrs];
    }
    
    if (_config.barBackgroundColor) {
        self.navigationController.navigationBar.barTintColor = _config.barBackgroundColor;
        self.navigationController.navigationBar.translucent = NO;
    }
    
    
    if (_config.displayURLAsPageTitle) {
        NSURL *URL = [NSURL URLWithString:_URL];
        if (URL != nil) {
            self.navigationItem.title = URL.host;
        }
        
    } else if (_config.pageTitle) {
        self.navigationItem.title = _config.pageTitle;
    }
    
    if (!_config.hidesHistoryButtons) {
        UIBarButtonItem* backButton = [[UIBarButtonItem alloc] initWithTitle:@"\u2190"
                                                                      style:UIBarButtonItemStylePlain
                                                                     target:self
                                                                     action:@selector(backwardButtonPressed)];
        UIBarButtonItem* forwardButton = [[UIBarButtonItem alloc] initWithTitle:@"\u2192"
                                                                      style:UIBarButtonItemStylePlain
                                                                     target:self
                                                                     action:@selector(forwardButtonPressed)];
        
        if (backButtonAttrs) {
            [backButton setTitleTextAttributes:backButtonAttrs forState:UIControlStateNormal];
            [forwardButton setTitleTextAttributes:backButtonAttrs forState:UIControlStateNormal];
        }
        
        backButton.enabled = NO;
        forwardButton.enabled = NO;
        
        [self.navigationItem setRightBarButtonItems:@[forwardButton, backButton]];
    }
}
    
- (void)updateNavButtons {
    NSArray *rightItems = self.navigationItem.rightBarButtonItems;
    BOOL hasNavButtons = rightItems.count == 2;
    
    if (hasNavButtons) {
        UIBarButtonItem *forward = rightItems[0];
        UIBarButtonItem *back = rightItems[1];
        
        forward.enabled = [_webView canGoForward];
        back.enabled = [_webView canGoBack];
    }
}

- (NSDictionary *)createBackButtonTitleAttributes {
    NSMutableDictionary *dict = [NSMutableDictionary new];
    if (_config.backButtonFontSize) {
        UIFont *font = [UIFont systemFontOfSize:CGFloat(_config.backButtonFontSize.floatValue)];
        [dict setObject:font forKey:NSFontAttributeName];
    }
    
    if (_config.textColor) {
        [dict setObject:_config.textColor forKey:NSForegroundColorAttributeName];
    }
    
    if (dict.count > 0) {
        return dict;
    } else {
        return nil;
    }
}

- (NSDictionary *)createTitleAttributes {
    NSMutableDictionary *dict = [NSMutableDictionary new];
    if (_config.textColor) {
        [dict setObject:_config.textColor forKey:NSForegroundColorAttributeName];
    }
    
    if (_config.titleFontSize) {
        UIFont *font = [UIFont systemFontOfSize:CGFloat(_config.titleFontSize.floatValue)];
        [dict setObject:font forKey:NSFontAttributeName];
    }
    
    if (dict.count > 0) {
        return dict;
    } else {
        return nil;
    }
}

- (BOOL)webView:(UIWebView *)webView shouldStartLoadWithRequest:(NSURLRequest *)request navigationType:(UIWebViewNavigationType)navigationType {
    NSString *scheme = request.URL.scheme;
    if ([scheme isEqualToString:@"inappbrowserbridge"]) {
        NSString *wholeMessage = [request.URL.absoluteString stringByReplacingOccurrencesOfString:@"inappbrowserbridge://" withString:@""];
        
        OnBrowserJSCallback([wholeMessage stringByReplacingPercentEscapesUsingEncoding:NSUTF8StringEncoding]);
        return NO;
    }
    return YES;
}


- (void)webViewDidFinishLoad:(UIWebView *)webView {
    [_indicatorView stopAnimating];
    [self updateNavButtons];
    OnBrowserFinishedLoading(webView.request);
}

- (void)webView:(UIWebView *)webView didFailLoadWithError:(NSError *)error {
    [_indicatorView stopAnimating];
    [self updateNavButtons];
    OnBrowserFinishedLoadingWithError(webView.request, error);
}

- (void)webViewDidStartLoad:(UIWebView *)webView {
    [self updateNavButtons];
    OnBrowserStartedLoading(webView.request);
}

- (void)backButtonPressed {
    [self.navigationController.presentingViewController dismissViewControllerAnimated:true completion:^{
        OnBrowserClosed();
    }];
}
    
- (void)backwardButtonPressed {
    [_webView goBack];
}
    
- (void)forwardButtonPressed {
    [_webView goForward];
}

- (BOOL)canGoBack {
    return [_webView canGoBack];
}

- (BOOL)canGoForward {
    return [_webView canGoForward];
}

- (void)goBack {
    [_webView goBack];
}

- (void)goForward {
    [_webView goForward];
}

@end
