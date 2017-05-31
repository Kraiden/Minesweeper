package nz.geek.nathan.minesweeper.app

import dagger.Component
import dagger.Module
import dagger.Provides
import nz.geek.nathan.minesweeper.splash.SplashComponent
import nz.geek.nathan.minesweeper.splash.SplashModule
import javax.inject.Singleton

/**
 * Created by nate on 17/05/17.
 */
@Singleton
@Component(modules = arrayOf(ApplicationModule::class))
interface ApplicationComponent{
    fun inject(app: MSApplication)

    fun plus(module: SplashModule): SplashComponent
}

@Module
class ApplicationModule(val mApplication: MSApplication){
    @Provides @Singleton
    fun provideApplication() = mApplication
}