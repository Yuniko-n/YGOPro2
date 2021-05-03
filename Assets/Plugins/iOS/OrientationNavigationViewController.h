//
//  OrientationNavigationViewController.h
//  Unity-iPhone
//
//  Created by Piotr on 16/07/17.
//
//

#import <UIKit/UIKit.h>

@interface OrientationNavigationViewController : UINavigationController
@property (nonatomic) BOOL shouldStickToPortrait;
@property (nonatomic) BOOL shouldStickToLandscape;
@end

@interface StandardNavigationController : UINavigationController
@end
