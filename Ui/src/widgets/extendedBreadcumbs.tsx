import { FC } from 'react';
import { Widget } from 'cs2/bindings';
import { getModule } from 'cs2/modding';
import classNames from "classnames";
import styles from './extendedKeybinding.module.scss';

export const ExtendedBreadcumbs = (data: Widget<{ icon?: string }>) => 
{
  const hasIcon = !!data.props.icon;
  return (
    <div className={classNames(styles.ksHotkeyBreadcumbs)}>
      {hasIcon && <div className={classNames(styles.ksHotkeyBreadcumbsIcon)}>
              <img src={data.props.icon} />
            </div>}
    </div>
  );
}